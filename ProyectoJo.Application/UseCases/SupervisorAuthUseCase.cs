using System.Security.Cryptography;
using ProyectoJo.Application.Ports.In;
using ProyectoJo.Application.Ports.Out;

namespace ProyectoJo.Application.UseCases
{
	public class SupervisorAuthUseCase : ISupervisorAuthService
	{
		private readonly IAdministradorRepository _administradorRepository;

		public SupervisorAuthUseCase(IAdministradorRepository administradorRepository)
		{
			_administradorRepository = administradorRepository;
		}

		public async Task<bool> ValidarClaveAsync(string clave)
		{
			if (string.IsNullOrWhiteSpace(clave)) return false;

			var administradores = await _administradorRepository.ObtenerTodosAsync();

			return administradores.Any(a =>
				a.Activo &&
				!string.IsNullOrWhiteSpace(a.ClaveSupervisorHash) &&
				VerificarClave(clave, a.ClaveSupervisorHash));
		}

		private static bool VerificarClave(string claveIngresada, string hashGuardado)
		{
			var partes = hashGuardado.Split('.');
			if (partes.Length != 2) return false;

			var salt = Convert.FromBase64String(partes[0]);
			var hashEsperado = Convert.FromBase64String(partes[1]);

			var hashIngresado = Rfc2898DeriveBytes.Pbkdf2(claveIngresada, salt, 100_000, HashAlgorithmName.SHA256, 32);

			return CryptographicOperations.FixedTimeEquals(hashIngresado, hashEsperado);
		}
	}
}
