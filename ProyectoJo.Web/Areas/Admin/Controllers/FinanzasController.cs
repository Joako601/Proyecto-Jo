using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoJo.Application.Ports.In;
using ProyectoJo.Domain.Entities;
using ProyectoJo.Application.DTOs;

namespace ProyectoJo.Web.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(AuthenticationSchemes = "JoCookieAuth")]
	public class FinanzasController : Controller
	{
		private readonly IFinanzaService _finanzaService;

		public FinanzasController(IFinanzaService finanzaService)
		{
			_finanzaService = finanzaService;
		}

		// GET: /Admin/Finanzas
		public IActionResult Index()
		{
			var movimientos = _finanzaService.ObtenerTodos();
			var resumen = _finanzaService.ObtenerResumenDelDia(DateTime.Today);
			ViewBag.Resumen = resumen;
			return View(movimientos);
		}

		// GET: /Admin/Finanzas/Registrar
		public IActionResult Registrar() => View();

		// POST: /Admin/Finanzas/Registrar
		[HttpPost]
		public IActionResult Registrar(Finanza finanza)
		{
			if (!ModelState.IsValid) return View(finanza);
			_finanzaService.RegistrarMovimiento(finanza);
			return RedirectToAction("Index");
		}

		// GET: /Admin/Finanzas/Editar/5
		public IActionResult Editar(int id)
		{
			var finanza = _finanzaService.ObtenerPorId(id);
			return finanza == null ? NotFound() : View(finanza);
		}

		// POST: /Admin/Finanzas/Editar
		[HttpPost]
		public IActionResult Editar(Finanza finanza)
		{
			if (!ModelState.IsValid) return View(finanza);
			_finanzaService.Editar(finanza);
			return RedirectToAction("Index");
		}

		// POST: /Admin/Finanzas/Eliminar/5
		[HttpPost]
		public IActionResult Eliminar(int id)
		{
			_finanzaService.Eliminar(id);
			return RedirectToAction("Index");
		}

		// GET: /Admin/Finanzas/Resumen
		public IActionResult Resumen(DateTime? desde, DateTime? hasta)
		{
			var inicio = desde ?? DateTime.Today.AddDays(-30);
			var fin = hasta ?? DateTime.Today;
			var resumen = _finanzaService.ObtenerResumenPorPeriodo(inicio, fin);
			ViewBag.Desde = inicio;
			ViewBag.Hasta = fin;
			return View(resumen);
		}
	}
}