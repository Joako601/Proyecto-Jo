using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using ProyectoJo.Application.Ports.In;
using System.Security.Claims;

namespace ProyectoJo.Web.Areas.Admin.Controllers
{
	[Area("Admin")]
	public class LoginController : Controller
	{
		private readonly IAuthService _authService;

		public LoginController(IAuthService authService)
		{
			_authService = authService;
		}

		public IActionResult Index() => View();

		[HttpPost]
		public async Task<IActionResult> Index(string usuario, string contrasena)
		{
			if (!_authService.ValidarCredenciales(usuario, contrasena))
			{
				ViewBag.Error = "Credenciales incorrectas";
				return View();
			}

			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.Name, usuario)
			};

			var identity = new ClaimsIdentity(claims, "JoCookieAuth");

			var principal = new ClaimsPrincipal(identity);

			await HttpContext.SignInAsync("JoCookieAuth", principal);

			return RedirectToAction("Index", "Gestion", new { area = "Admin" });
		}

		public async Task<IActionResult> Salir()
		{
			await HttpContext.SignOutAsync("JoCookieAuth");
			return RedirectToAction("Index");
		}
	}
}