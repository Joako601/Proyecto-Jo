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
			Converters = { new JsonStringEnumConverter() },
			WriteIndented = true
		};

		public JsonEmpleadoRepository(string rutaArchivo)
		{
			_rutaArchivo = rutaArchivo;
		}

		public async Task<List<Empleado>> ObtenerTodosAsync()
		{
			await _lock.WaitAsync();
			try { return await LeerSinLockAsync(); }
			finally { _lock.Release(); }
		}

		public async Task<Empleado?> ObtenerPorIdAsync(int id)
		{
			var todos = await ObtenerTodosAsync();
			return todos.FirstOrDefault(e => e.Id == id);
		}

		public async Task AgregarAsync(Empleado empleado)
		{
			await _lock.WaitAsync();
			try
			{
				var todos = await LeerSinLockAsync();
				empleado.Id = todos.Count == 0 ? 1 : todos.Max(e => e.Id) + 1;
				todos.Add(empleado);
				await GuardarSinLockAsync(todos);
			}
			finally { _lock.Release(); }
		}

		public async Task<bool> ActualizarAsync(Empleado empleado)
		{
			await _lock.WaitAsync();
			try
			{
				var todos = await LeerSinLockAsync();
				var index = todos.FindIndex(e => e.Id == empleado.Id);
				if (index == -1) return false;

				todos[index] = empleado;
				await GuardarSinLockAsync(todos);
				return true;
			}
			finally { _lock.Release(); }
		}

		public async Task<bool> EliminarAsync(int id)
		{
			await _lock.WaitAsync();
			try
			{
				var todos = await LeerSinLockAsync();
				if (todos.RemoveAll(e => e.Id == id) == 0) return false;

				await GuardarSinLockAsync(todos);
				return true;
			}
			finally { _lock.Release(); }
		}

		private async Task<List<Empleado>> LeerSinLockAsync()
		{
			if (!File.Exists(_rutaArchivo)) return new List<Empleado>();
			var json = await File.ReadAllTextAsync(_rutaArchivo);
			return JsonSerializer.Deserialize<List<Empleado>>(json, _options) ?? new List<Empleado>();
		}

		private async Task GuardarSinLockAsync(List<Empleado> empleados)
		{
			var json = JsonSerializer.Serialize(empleados, _options);
			await File.WriteAllTextAsync(_rutaArchivo, json);
		}
	}
}