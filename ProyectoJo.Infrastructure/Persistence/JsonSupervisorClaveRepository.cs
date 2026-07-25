using System.Text.Json;
using ProyectoJo.Application.Ports.Out;

namespace ProyectoJo.Infrastructure.Persistence
{
	public class JsonSupervisorClaveRepository : ISupervisorClaveRepository
	{
		private readonly string _rutaArchivo;
		private static readonly SemaphoreSlim _lock = new(1, 1);

		private static readonly JsonSerializerOptions _options = new()
		{
			WriteIndented = true
		};

		private class ClaveDto
		{
			public string ClaveHash { get; set; } = string.Empty;
		}

		public JsonSupervisorClaveRepository(string rutaArchivo)
		{
			_rutaArchivo = rutaArchivo;
		}

		public async Task<string?> ObtenerHashAsync()
		{
			await _lock.WaitAsync();
			try
			{
				var dto = await LeerAsync();
				return string.IsNullOrWhiteSpace(dto?.ClaveHash) ? null : dto.ClaveHash;
			}
			finally
			{
				_lock.Release();
			}
		}

		public async Task GuardarHashAsync(string hash)
		{
			await _lock.WaitAsync();
			try
			{
				await PersistirAsync(new ClaveDto { ClaveHash = hash });
			}
			finally
			{
				_lock.Release();
			}
		}

		private async Task<ClaveDto?> LeerAsync()
		{
			if (!File.Exists(_rutaArchivo)) return null;
			var json = await File.ReadAllTextAsync(_rutaArchivo);
			return JsonSerializer.Deserialize<ClaveDto>(json, _options);
		}

		private async Task PersistirAsync(ClaveDto dto)
		{
			var json = JsonSerializer.Serialize(dto, _options);
			var rutaTemporal = _rutaArchivo + ".tmp";
			await File.WriteAllTextAsync(rutaTemporal, json);
			File.Move(rutaTemporal, _rutaArchivo, overwrite: true);
		}
	}
}