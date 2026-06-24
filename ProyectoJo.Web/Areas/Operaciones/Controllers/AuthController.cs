using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using ProyectoJo.Application.Ports.In;
using ProyectoJo.Domain.Entities;
using System.Security.Claims;

namespace ProyectoJo.Web.Areas.Operaciones.Controllers
{
	[Area("Operaciones")]
	public class AuthController : Controller
	{
		private readonly IEmpleadoAuthService _empleadoAuthService;
		private readonly IDispositivoService _dispositivoService;

		public AuthController(IEmpleadoAuthService empleadoAuthService, IDispositivoService dispositivoService)
		{
			_empleadoAuthService = empleadoAuthService;
			_dispositivoService = dispositivoService;
		}

		public async Task<IActionResult> Login()
		{
			var token = Request.Cookies["Jo.DispositivoToken"];
			var dispositivo = token is null ? null : await _dispositivoService.ReconocerAsync(token);

			if (dispositivo is null)
				return RedirectToAction("Emparejar");

			ViewBag.Estacion = dispositivo.Estacion;
			ViewBag.NombreDispositivo = dispositivo.Nombre;
			return View();
		}

		// POST /Operaciones/Auth/Login
		[HttpPost]
		public async Task<IActionResult> Login(string pin)
		{
			var token = Request.Cookies["Jo.DispositivoToken"];
			var dispositivo = token is null ? null : await _dispositivoService.ReconocerAsync(token);

			if (dispositivo is null)
				return RedirectToAction("Emparejar");

			var empleado = await _empleadoAuthService.ValidarPinAsync(pin, dispositivo.Estacion);
			if (empleado is null)
			{
				ViewBag.Error = "PIN incorrecto";
				ViewBag.Estacion = dispositivo.Estacion;
				ViewBag.NombreDispositivo = dispositivo.Nombre;
				return View();
			}

			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.Name, empleado.Nombre),
				new Claim(ClaimTypes.Role, empleado.Rol.ToString())
			};

			var identity = new ClaimsIdentity(claims, "OperacionesCookieAuth");
			var principal = new ClaimsPrincipal(identity);

			await HttpContext.SignInAsync("OperacionesCookieAuth", principal,
				new AuthenticationProperties
				{
					IsPersistent = true,
					ExpiresUtc = DateTimeOffset.UtcNow.AddHours(12)
				});

			return dispositivo.Estacion == RolEmpleado.Cocina
				? RedirectToAction("Index", "Cocina")
				: RedirectToAction("Index", "Recepcion");
		}

		public IActionResult Emparejar() => View();

		[HttpPost]
		public async Task<IActionResult> Emparejar(RolEmpleado estacion, string nombre)
		{
			var dispositivo = await _dispositivoService.EmparejarAsync(estacion, nombre);

			Response.Cookies.Append("Jo.DispositivoToken", dispositivo.Token, new CookieOptions
			{
				Expires = DateTimeOffset.UtcNow.AddYears(5),
				HttpOnly = true,
				IsEssential = true,
				SameSite = SameSiteMode.Lax
			});

			return RedirectToAction("Login");
		}

		public async Task<IActionResult> Salir()
		{
			await HttpContext.SignOutAsync("OperacionesCookieAuth");
			return RedirectToAction("Login");
		}
	}
}