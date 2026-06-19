using Microsoft.Extensions.Configuration;
using ProyectoJo.Application.Ports.In;

namespace ProyectoJo.Infrastructure.Auth
{
	public class EnvAuthService : IAuthService
	{
		private readonly string _usuario;
		private readonly string _contrasena;

		public EnvAuthService(IConfiguration configuration)
		{
			_usuario = configuration["Auth:AdminUser"] ?? "";
			_contrasena = configuration["Auth:AdminPassword"] ?? "";
		}

		public bool ValidarCredenciales(string usuario, string contrasena)
		{
			return usuario == _usuario && contrasena == _contrasena;
		}
	}
}
