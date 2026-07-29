using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
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

		public IActionResult Index(bool bloqueado = false, string? returnUrl = null)
		{
			if (bloqueado)
				ViewBag.Error = "Demasiados intentos. Espera un momento antes de volver a intentar.";

			ViewBag.ReturnUrl = (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)) ? returnUrl : null;
			return View();
		}

		[HttpPost]
		[EnableRateLimiting("login-admin")]
		public async Task<IActionResult> Index(string usuario, string contrasena, string? returnUrl = null)
		{
			var resultado = await _authService.ValidarCredencialesAsync(usuario, contrasena);
			if (resultado is null)
			{
				ViewBag.Error = "Credenciales incorrectas";
				return View();
			}

			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.Name, resultado.Usuario),
				new Claim(ClaimTypes.Role, resultado.Rol)
			};

			var identity = new ClaimsIdentity(claims, "JoCookieAuth");
			var principal = new ClaimsPrincipal(identity);

			await HttpContext.SignInAsync("JoCookieAuth", principal);

			if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
				return Redirect(returnUrl);

			return RedirectToAction("Index", "Gestion", new { area = "Admin" });
		}

		public async Task<IActionResult> Salir()
		{
			await HttpContext.SignOutAsync("JoCookieAuth");
			return RedirectToAction("Index");
		}
	}
}