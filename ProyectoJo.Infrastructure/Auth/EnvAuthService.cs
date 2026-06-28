using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using ProyectoJo.Application.Ports.In;

namespace ProyectoJo.Infrastructure.Auth
{
	public class EnvAuthService : IAuthService
	{
		private readonly string _usuario;
		private readonly string _contrasenaHash;

		public EnvAuthService(IConfiguration configuration)
		{
			_usuario = configuration["Auth:AdminUser"] ?? "";
			_contrasenaHash = configuration["Auth:AdminPasswordHash"] ?? "";
		}

		public bool ValidarCredenciales(string usuario, string contrasena)
		{
			if (string.IsNullOrWhiteSpace(_contrasenaHash)) return false;
			if (usuario != _usuario) return false;

			return VerificarContrasena(contrasena, _contrasenaHash);
		}

		private static bool VerificarContrasena(string contrasenaIngresada, string hashGuardado)
		{
			var partes = hashGuardado.Split('.');
			if (partes.Length != 2) return false;

			var salt = Convert.FromBase64String(partes[0]);
			var hashEsperado = Convert.FromBase64String(partes[1]);

			var hashIngresado = Rfc2898DeriveBytes.Pbkdf2(contrasenaIngresada, salt, 100_000, HashAlgorithmName.SHA256, 32);

			return CryptographicOperations.FixedTimeEquals(hashIngresado, hashEsperado);
		}
	}
}