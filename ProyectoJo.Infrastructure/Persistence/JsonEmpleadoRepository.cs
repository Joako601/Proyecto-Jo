using System.Text.Json;
using System.Text.Json.Serialization;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Infrastructure.Persistence
{
	public class JsonEmpleadoRepository : IEmpleadoRepository
	{
		private readonly string _rutaArchivo;
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
			if (!File.Exists(_rutaArchivo)) return new List<Empleado>();
			var json = await File.ReadAllTextAsync(_rutaArchivo);
			return JsonSerializer.Deserialize<List<Empleado>>(json, _options) ?? new List<Empleado>();
		}

		public async Task<Empleado?> ObtenerPorIdAsync(int id)
		{
			var todos = await ObtenerTodosAsync();
			return todos.FirstOrDefault(e => e.Id == id);
		}
	}
}