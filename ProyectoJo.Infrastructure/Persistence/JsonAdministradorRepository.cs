using System.Text.Json;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Infrastructure.Persistence
{
	public class JsonAdministradorRepository : IAdministradorRepository
	{
		private readonly string _rutaArchivo;
		private static readonly SemaphoreSlim _lock = new(1, 1);
		private static readonly JsonSerializerOptions _options = new() { WriteIndented = true };

		public JsonAdministradorRepository(string rutaArchivo)
		{
			_rutaArchivo = rutaArchivo;
		}

		public async Task<List<Administrador>> ObtenerTodosAsync()
		{
			await _lock.WaitAsync();
			try { return await LeerSinLockAsync(); }
			finally { _lock.Release(); }
		}

		public async Task<Administrador?> ObtenerPorIdAsync(int id)
		{
			var todos = await ObtenerTodosAsync();
			return todos.FirstOrDefault(a => a.Id == id);
		}

		public async Task<Administrador?> ObtenerPorUsuarioAsync(string usuario)
		{
			var todos = await ObtenerTodosAsync();
			return todos.FirstOrDefault(a => a.Usuario.Equals(usuario, StringComparison.OrdinalIgnoreCase));
		}

		public async Task AgregarAsync(Administrador administrador)
		{
			await _lock.WaitAsync();
			try
			{
				var todos = await LeerSinLockAsync();
				administrador.Id = todos.Count == 0 ? 1 : todos.Max(a => a.Id) + 1;
				todos.Add(administrador);
				await GuardarSinLockAsync(todos);
			}
			finally { _lock.Release(); }
		}

		public async Task<bool> ActualizarAsync(Administrador administrador)
		{
			await _lock.WaitAsync();
			try
			{
				var todos = await LeerSinLockAsync();
				var index = todos.FindIndex(a => a.Id == administrador.Id);
				if (index == -1) return false;

				todos[index] = administrador;
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
				if (todos.RemoveAll(a => a.Id == id) == 0) return false;

				await GuardarSinLockAsync(todos);
				return true;
			}
			finally { _lock.Release(); }
		}

		private async Task<List<Administrador>> LeerSinLockAsync()
		{
			if (!File.Exists(_rutaArchivo)) return new List<Administrador>();
			var json = await File.ReadAllTextAsync(_rutaArchivo);
			return JsonSerializer.Deserialize<List<Administrador>>(json, _options) ?? new List<Administrador>();
		}

		private async Task GuardarSinLockAsync(List<Administrador> administradores)
		{
			var json = JsonSerializer.Serialize(administradores, _options);
			await File.WriteAllTextAsync(_rutaArchivo, json);
		}
	}
}