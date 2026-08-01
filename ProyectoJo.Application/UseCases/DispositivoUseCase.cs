using System.Security.Cryptography;
using ProyectoJo.Application.Ports.In;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Application.UseCases
{
	public class DispositivoUseCase : IDispositivoService
	{
		private readonly IDispositivoRepository _repository;

		public DispositivoUseCase(IDispositivoRepository repository)
		{
			_repository = repository;
		}

		public async Task<DispositivoOperaciones> EmparejarAsync(RolEmpleado estacion, string nombre)
		{
			var dispositivo = new DispositivoOperaciones
			{
				Token = GenerarToken(),
				Estacion = estacion,
				Nombre = nombre,
				FechaRegistro = DateTime.UtcNow
			};

			return await _repository.RegistrarAsync(dispositivo);
		}

		public async Task<DispositivoOperaciones?> ReasignarEstacionAsync(string token, RolEmpleado estacion, string? nombre)
		{
			if (string.IsNullOrWhiteSpace(token)) return null;
			return await _repository.ActualizarEstacionAsync(token, estacion, nombre);
		}

		public async Task<DispositivoOperaciones?> ReconocerAsync(string token)
		{
			if (string.IsNullOrWhiteSpace(token)) return null;
			return await _repository.ObtenerPorTokenAsync(token);
		}

		public async Task<List<DispositivoOperaciones>> ObtenerTodosAsync()
		{
			return await _repository.ObtenerTodosAsync();
		}

		public async Task<DispositivoOperaciones?> ToggleBloqueadoAsync(int id)
		{
			return await _repository.ToggleBloqueadoAsync(id);
		}

		public async Task<DispositivoOperaciones?> ToggleActivoAsync(int id)
		{
			return await _repository.ToggleActivoAsync(id);
		}

		private static string GenerarToken()
		{
			return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
				.Replace("+", "")
				.Replace("/", "")
				.Replace("=", "");
		}
	}
}