using System.Security.Claims;
using ProyectoJo.Infrastructure.Auth;

namespace ProyectoJo.Web.Authorization
{
	public static class ClaimsPrincipalExtensions
	{

		public static bool TieneAccesoArea(this ClaimsPrincipal usuario, string area)
		{
			if (usuario.Identity is null || !usuario.Identity.IsAuthenticated)
				return false;

			if (usuario.IsInRole(EnvAuthService.RolSuperAdmin))
				return true;

			if (!usuario.IsInRole(EnvAuthService.RolAdministrador))
				return false;

			var areas = usuario.FindAll("Area").Select(c => c.Value);

			return areas.Contains("General") || areas.Contains(area);
		}
	}
}