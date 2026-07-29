using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoJo.Application.Ports.In;
using ProyectoJo.Web.Authorization;

namespace ProyectoJo.Web.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(AuthenticationSchemes = "JoCookieAuth")]
	[RequiereArea("Dispositivos")]
	public class DispositivosController : Controller
	{
		private readonly IDispositivoService _dispositivoService;
		private readonly ISupervisorAuthService _supervisorAuthService;

		public DispositivosController(IDispositivoService dispositivoService, ISupervisorAuthService supervisorAuthService)
		{
			_dispositivoService = dispositivoService;
			_supervisorAuthService = supervisorAuthService;
		}

		public async Task<IActionResult> Index()
		{
			var dispositivos = await _dispositivoService.ObtenerTodosAsync();
			ViewBag.ClaveConfigurada = await _supervisorAuthService.TieneClaveConfiguradaAsync();
			return View(dispositivos.OrderByDescending(d => d.FechaRegistro).ToList());
		}

		// POST /Admin/Dispositivos/CambiarClaveSupervisor
		[HttpPost]
		public async Task<IActionResult> CambiarClaveSupervisor(string? claveActual, string claveNueva, string confirmarClaveNueva)
		{
			if (claveNueva != confirmarClaveNueva)
			{
				TempData["ErrorClave"] = "La confirmación no coincide con la clave nueva.";
				return RedirectToAction("Index");
			}

			var exito = await _supervisorAuthService.CambiarClaveAsync(claveActual, claveNueva);

			TempData["ErrorClave"] = exito ? null : "No se pudo cambiar la clave. Revisá la clave actual e intentá de nuevo (mínimo 6 caracteres).";
			TempData["ExitoClave"] = exito ? "Clave de supervisor actualizada correctamente." : null;

			return RedirectToAction("Index");
		}
	}
}