using ProyectoJo.Application.Ports.In;

namespace ProyectoJo.Infrastructure.Auth
{
	public class EnvAuthService : IAuthService
	{
		private readonly string _usuario;
		private readonly string _contrasena;

		public EnvAuthService()
		{
			_usuario = Environment.GetEnvironmentVariable("JO_ADMIN_USER") ?? "";
			_contrasena = Environment.GetEnvironmentVariable("JO_ADMIN_PASSWORD") ?? "";
		}

		public bool ValidarCredenciales(string usuario, string contrasena)
		{
			return usuario == _usuario && contrasena == _contrasena;
		}
	}
}
