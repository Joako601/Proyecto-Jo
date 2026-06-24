using System.Text.Json;
using System.Text.Json.Serialization;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Infrastructure.Persistence
{
	public class JsonDispositivoRepository : IDispositivoRepository
	{
		private readonly string _rutaArchivo;
		private static readonly SemaphoreSlim _lock = new(1, 1);

		private static readonly JsonSerializerOptions _options = new()
		{
			WriteIndented = true,
			Converters = { new JsonStringEnumConverter() }
		};

		public JsonDispositivoRepository(string rutaArchivo)
		{
			_rutaArchivo = rutaArchivo;
		}

		public async Task<DispositivoOperaciones?> ObtenerPorTokenAsync(string token)
		{
			await _lock.WaitAsync();
			try
			{
				var todos = await LeerAsync();
				return todos.FirstOrDefault(d => d.Token == token);
			}
			finally
			{
				_lock.Release();
			}
		}

		public async Task<DispositivoOperaciones> RegistrarAsync(DispositivoOperaciones dispositivo)
		{
			await _lock.WaitAsync();
			try
			{
				var todos = await LeerAsync();
				todos.Add(dispositivo);
				await PersistirAsync(todos);
				return dispositivo;
			}
			finally
			{
				_lock.Release();
			}
		}

		private async Task<List<DispositivoOperaciones>> LeerAsync()
		{
			if (!File.Exists(_rutaArchivo)) return new List<DispositivoOperaciones>();
			var json = await File.ReadAllTextAsync(_rutaArchivo);
			return JsonSerializer.Deserialize<List<DispositivoOperaciones>>(json, _options) ?? new List<DispositivoOperaciones>();
		}

		private async Task PersistirAsync(List<DispositivoOperaciones> lista)
		{
			var json = JsonSerializer.Serialize(lista, _options);
			await File.WriteAllTextAsync(_rutaArchivo, json);
		}
	}
}