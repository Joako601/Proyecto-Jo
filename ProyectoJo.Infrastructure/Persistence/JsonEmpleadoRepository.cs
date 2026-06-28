using System.Text.Json;
using System.Text.Json.Serialization;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Infrastructure.Persistence
{
	public class JsonEmpleadoRepository : IEmpleadoRepository
	{
		private readonly string _rutaArchivo;
		private static readonly SemaphoreSlim _lock = new(1, 1);
		private static readonly JsonSerializerOptions _options = new()
		{
			Converters = { new JsonStringEnumConverter() }
		};

		public JsonEmpleadoRepository(string rutaArchivo)
		{
			_rutaArchivo = rutaArchivo;
		}

		public async Task<List<Empleado>> ObtenerTodosAsync()
		{
			await _lock.WaitAsync();
			try
			{
				if (!File.Exists(_rutaArchivo)) return new List<Empleado>();
				var json = await File.ReadAllTextAsync(_rutaArchivo);
				return JsonSerializer.Deserialize<List<Empleado>>(json, _options) ?? new List<Empleado>();
			}
			finally
			{
				_lock.Release();
			}
		}

		public async Task<Empleado?> ObtenerPorIdAsync(int id)
		{
			var todos = await ObtenerTodosAsync();
			return todos.FirstOrDefault(e => e.Id == id);
		}
	}
}