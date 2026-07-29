using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
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
		private readonly ISupervisorAuthService _supervisorAuthService;

		public AuthController(
			IEmpleadoAuthService empleadoAuthService,
			IDispositivoService dispositivoService,
			ISupervisorAuthService supervisorAuthService)
		{
			_empleadoAuthService = empleadoAuthService;
			_dispositivoService = dispositivoService;
			_supervisorAuthService = supervisorAuthService;
		}

		public async Task<IActionResult> Login(bool bloqueado = false)
		{
			var token = Request.Cookies["Jo.DispositivoToken"];
			var dispositivo = token is null ? null : await _dispositivoService.ReconocerAsync(token);

			if (dispositivo is null)
				return RedirectToAction("Emparejar");

			ViewBag.Bloqueado = bloqueado;
			if (bloqueado)
				ViewBag.Error = "Demasiados intentos. Espera un momento antes de volver a intentar.";

			ViewBag.Estacion = dispositivo.Estacion;
			ViewBag.NombreDispositivo = dispositivo.Nombre;
			return View();
		}

		// POST /Operaciones/Auth/Login
		[HttpPost]
		[EnableRateLimiting("login-operador")]
		public async Task<IActionResult> Login(string nombre, string clave)
		{
			var token = Request.Cookies["Jo.DispositivoToken"];
			var dispositivo = token is null ? null : await _dispositivoService.ReconocerAsync(token);

			if (dispositivo is null)
				return RedirectToAction("Emparejar");

			var empleado = await _empleadoAuthService.ValidarCredencialesAsync(nombre, clave, dispositivo.Estacion);
			if (empleado is null)
			{
				ViewBag.Error = "Nombre o clave incorrectos";
				ViewBag.Estacion = dispositivo.Estacion;
				ViewBag.NombreDispositivo = dispositivo.Nombre;
				return View();
			}

			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.Name, empleado.Nombre),
				new Claim(ClaimTypes.Role, empleado.Rol.ToString()),
				new Claim("Dispositivo", dispositivo.Nombre ?? string.Empty)
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

		// GET /Operaciones/Auth/LoginSupervisor
		public IActionResult LoginSupervisor(bool bloqueado = false)
		{
			ViewBag.Bloqueado = bloqueado;
			if (bloqueado)
				ViewBag.Error = "Demasiados intentos. Espera un momento antes de volver a intentar.";

			return View();
		}

		// POST /Operaciones/Auth/LoginSupervisor
		[HttpPost]
		[EnableRateLimiting("login-supervisor")]
		public async Task<IActionResult> LoginSupervisor(string clave)
		{
			if (!await _supervisorAuthService.ValidarClaveAsync(clave))
			{
				ViewBag.Error = "Clave incorrecta";
				return View();
			}

			var identity = new ClaimsIdentity(
				new[] { new Claim(ClaimTypes.Name, "Supervisor") },
				"SupervisorAuth");
			var principal = new ClaimsPrincipal(identity);

			await HttpContext.SignInAsync("SupervisorAuth", principal,
				new AuthenticationProperties { IsPersistent = false });

			return RedirectToAction("Emparejar");
		}

		// GET /Operaciones/Auth/Emparejar
		[Authorize(AuthenticationSchemes = "SupervisorAuth")]
		public IActionResult Emparejar() => View();

		// POST /Operaciones/Auth/Emparejar
		[HttpPost]
		[Authorize(AuthenticationSchemes = "SupervisorAuth")]
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

			await HttpContext.SignOutAsync("SupervisorAuth");

			return RedirectToAction("Login");
		}

		// GET /Operaciones/Auth/SalirSupervisor
		public async Task<IActionResult> SalirSupervisor()
		{
			await HttpContext.SignOutAsync("SupervisorAuth");
			return RedirectToAction("LoginSupervisor");
		}

		public async Task<IActionResult> Salir()
		{
			await HttpContext.SignOutAsync("OperacionesCookieAuth");
			return RedirectToAction("Login");
		}
	}
}