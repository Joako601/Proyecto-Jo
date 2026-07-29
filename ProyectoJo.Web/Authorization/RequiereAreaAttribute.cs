using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ProyectoJo.Infrastructure.Auth;

namespace ProyectoJo.Web.Authorization
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public class RequiereAreaAttribute : Attribute, IAuthorizationFilter
	{
		private readonly string _area;

		public RequiereAreaAttribute(string area)
		{
			_area = area;
		}

		public void OnAuthorization(AuthorizationFilterContext context)
		{
			var usuario = context.HttpContext.User;
			if (usuario.Identity is null || !usuario.Identity.IsAuthenticated)
				return; 

			if (usuario.IsInRole(EnvAuthService.RolSuperAdmin))
				return;

			if (!usuario.IsInRole(EnvAuthService.RolAdministrador))
			{
				context.Result = new ForbidResult("JoCookieAuth");
				return;
			}

			var areas = usuario.FindAll("Area").Select(c => c.Value).ToList();

			if (areas.Contains("General") || areas.Contains(_area))
				return;

			context.Result = new ForbidResult("JoCookieAuth");
		}
	}
}