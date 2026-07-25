using System.Security.Cryptography;
using ProyectoJo.Application.Ports.In;
using ProyectoJo.Application.Ports.Out;

namespace ProyectoJo.Application.UseCases
{
	public class SupervisorAuthUseCase : ISupervisorAuthService
	{
		private readonly ISupervisorClaveRepository _repository;

		private const int SaltSize = 16;
		private const int HashSize = 32;
		private const int Iteraciones = 100_000;

		public SupervisorAuthUseCase(ISupervisorClaveRepository repository)
		{
			_repository = repository;
		}

		public async Task<bool> ValidarClaveAsync(string clave)
		{
			if (string.IsNullOrWhiteSpace(clave)) return false;

			var hashGuardado = await _repository.ObtenerHashAsync();
			if (string.IsNullOrWhiteSpace(hashGuardado)) return false;

			return VerificarClave(clave, hashGuardado);
		}

		public async Task<bool> TieneClaveConfiguradaAsync()
		{
			var hashGuardado = await _repository.ObtenerHashAsync();
			return !string.IsNullOrWhiteSpace(hashGuardado);
		}

		public async Task<bool> CambiarClaveAsync(string? claveActual, string claveNueva)
		{
			if (string.IsNullOrWhiteSpace(claveNueva) || claveNueva.Length < 6) return false;

			var hashGuardado = await _repository.ObtenerHashAsync();

			if (!string.IsNullOrWhiteSpace(hashGuardado))
			{
				if (string.IsNullOrWhiteSpace(claveActual) || !VerificarClave(claveActual, hashGuardado))
					return false;
			}

			await _repository.GuardarHashAsync(HashearClave(claveNueva));
			return true;
		}

		private static string HashearClave(string clave)
		{
			var salt = RandomNumberGenerator.GetBytes(SaltSize);
			var hash = Rfc2898DeriveBytes.Pbkdf2(clave, salt, Iteraciones, HashAlgorithmName.SHA256, HashSize);
			return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
		}

		private static bool VerificarClave(string claveIngresada, string hashGuardado)
		{
			var partes = hashGuardado.Split('.');
			if (partes.Length != 2) return false;

			var salt = Convert.FromBase64String(partes[0]);
			var hashEsperado = Convert.FromBase64String(partes[1]);

			var hashIngresado = Rfc2898DeriveBytes.Pbkdf2(claveIngresada, salt, Iteraciones, HashAlgorithmName.SHA256, HashSize);

			return CryptographicOperations.FixedTimeEquals(hashIngresado, hashEsperado);
		}
	}
}