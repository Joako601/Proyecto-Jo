using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoJo.Application.Ports.In;
using ProyectoJo.Web.Authorization;

namespace ProyectoJo.Web.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(AuthenticationSchemes = "JoCookieAuth")]
	[RequiereArea("CierreCaja")]
	public class CierreCajaController : Controller
	{
		private readonly ICierreCajaService _cierreCajaService;

		public CierreCajaController(ICierreCajaService cierreCajaService)
		{
			_cierreCajaService = cierreCajaService;
		}

		// GET: /Admin/CierreCaja
		public IActionResult Index()
		{
			ViewBag.CajaAbierta = _cierreCajaService.ObtenerCajaAbierta();
			var historial = _cierreCajaService.ObtenerHistorial();
			return View(historial);
		}

		// GET: /Admin/CierreCaja/Abrir
		public IActionResult Abrir() => View();

		// POST: /Admin/CierreCaja/Abrir
		[HttpPost]
		public IActionResult Abrir(decimal fondoInicial, string? notas)
		{
			try
			{
				_cierreCajaService.AbrirCaja(fondoInicial, notas, User.Identity?.Name ?? "Desconocido");
				return RedirectToAction("Index");
			}
			catch (InvalidOperationException ex)
			{
				ModelState.AddModelError(string.Empty, ex.Message);
				return View();
			}
		}

		// GET: /Admin/CierreCaja/Cerrar/5
		public IActionResult Cerrar(int id)
		{
			try
			{
				var vistaPrevia = _cierreCajaService.ObtenerVistaPreviaCierre(id);
				return View(vistaPrevia);
			}
			catch (InvalidOperationException ex)
			{
				TempData["Error"] = ex.Message;
				return RedirectToAction("Index");
			}
		}

		// POST: /Admin/CierreCaja/Cerrar/5
		[HttpPost]
		public IActionResult Cerrar(int id, string? notas)
		{
			try
			{
				_cierreCajaService.CerrarCaja(id, notas, User.Identity?.Name ?? "Desconocido");
				return RedirectToAction("Index");
			}
			catch (InvalidOperationException ex)
			{
				TempData["Error"] = ex.Message;
				return RedirectToAction("Index");
			}
		}
	}
}