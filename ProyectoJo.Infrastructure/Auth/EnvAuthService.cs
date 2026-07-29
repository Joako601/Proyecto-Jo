using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using ProyectoJo.Application.Ports.In;
using ProyectoJo.Application.Ports.Out;

namespace ProyectoJo.Infrastructure.Auth
{
	public class EnvAuthService : IAuthService
	{
		private readonly string _usuario;
		private readonly string _contrasenaHash;
		private readonly IAdministradorRepository _administradorRepository;

		public const string RolSuperAdmin = "SuperAdmin";
		public const string RolAdministrador = "Administrador";

		public EnvAuthService(IConfiguration configuration, IAdministradorRepository administradorRepository)
		{
			_usuario = configuration["Auth:AdminUser"] ?? "";
			_contrasenaHash = configuration["Auth:AdminPasswordHash"] ?? "";
			_administradorRepository = administradorRepository;
		}

		public async Task<ResultadoAuth?> ValidarCredencialesAsync(string usuario, string contrasena)
		{
			if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(contrasena))
				return null;

			if (!string.IsNullOrWhiteSpace(_contrasenaHash) &&
				usuario == _usuario &&
				VerificarHash(contrasena, _contrasenaHash))
			{
				return new ResultadoAuth(usuario, RolSuperAdmin, new List<string>());
			}

			var administrador = await _administradorRepository.ObtenerPorUsuarioAsync(usuario);
			if (administrador is not null && administrador.Activo &&
				VerificarHash(contrasena, administrador.ContrasenaHash))
			{
				return new ResultadoAuth(administrador.Usuario, RolAdministrador, administrador.Areas);
			}

			return null;
		}

		private static bool VerificarHash(string valorIngresado, string hashGuardado)
		{
			var partes = hashGuardado.Split('.');
			if (partes.Length != 2) return false;

			var salt = Convert.FromBase64String(partes[0]);
			var hashEsperado = Convert.FromBase64String(partes[1]);

			var hashIngresado = Rfc2898DeriveBytes.Pbkdf2(valorIngresado, salt, 100_000, HashAlgorithmName.SHA256, 32);

			return CryptographicOperations.FixedTimeEquals(hashIngresado, hashEsperado);
		}
	}
}