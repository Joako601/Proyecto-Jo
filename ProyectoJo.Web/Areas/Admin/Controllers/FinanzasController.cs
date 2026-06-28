using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoJo.Application.Ports.In;
using ProyectoJo.Domain.Entities;

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
		public IActionResult Index(int? mes, int? anio)
		{
			var hoy = DateTime.Today;
			var mesActual = mes ?? hoy.Month;
			var anioActual = anio ?? hoy.Year;

			var movimientos = _finanzaService.ObtenerTodos()
				.Where(f => f.Fecha.Month == mesActual && f.Fecha.Year == anioActual)
				.OrderByDescending(f => f.Fecha)
				.ToList();

			var resumen = _finanzaService.ObtenerResumenDelDia(hoy);
			ViewBag.Resumen = resumen;
			ViewBag.Mes = mesActual;
			ViewBag.Anio = anioActual;

			return View(movimientos);
		}

		// GET: /Admin/Finanzas/Registrar
		public IActionResult Registrar() => View();

		// POST: /Admin/Finanzas/Registrar
		[HttpPost]
		public IActionResult Registrar(Finanza finanza)
		{
			if (!ModelState.IsValid) return View(finanza);
			_finanzaService.RegistrarMovimiento(finanza, User.Identity?.Name ?? "Desconocido");
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
			_finanzaService.Editar(finanza, User.Identity?.Name ?? "Desconocido");
			return RedirectToAction("Index");
		}

		// POST: /Admin/Finanzas/Eliminar/5
		[HttpPost]
		public IActionResult Eliminar(int id)
		{
			_finanzaService.Eliminar(id, User.Identity?.Name ?? "Desconocido");
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

		// GET: /Admin/Finanzas/Dashboard
		public IActionResult Dashboard()
		{
			var dashboard = _finanzaService.ObtenerDashboard();

			ViewBag.LabelesMeses = System.Text.Json.JsonSerializer.Serialize(dashboard.TendenciaAnio.Select(t => t.Etiqueta));
			ViewBag.DataIngresosAnio = System.Text.Json.JsonSerializer.Serialize(dashboard.TendenciaAnio.Select(t => t.Ingresos));
			ViewBag.DataIngresos = System.Text.Json.JsonSerializer.Serialize(dashboard.UltimosSeisMeses.Select(t => t.Ingresos));
			ViewBag.DataEgresos = System.Text.Json.JsonSerializer.Serialize(dashboard.UltimosSeisMeses.Select(t => t.Egresos));
			ViewBag.LabelsCategorias = System.Text.Json.JsonSerializer.Serialize(dashboard.TopCategorias.Select(c => c.Categoria));
			ViewBag.DataCategorias = System.Text.Json.JsonSerializer.Serialize(dashboard.TopCategorias.Select(c => c.Total));
			ViewBag.LabelsCategoriasIngresos = System.Text.Json.JsonSerializer.Serialize(dashboard.TopCategoriasIngresos.Select(c => c.Categoria));
			ViewBag.DataCategoriasIngresos = System.Text.Json.JsonSerializer.Serialize(dashboard.TopCategoriasIngresos.Select(c => c.Total));

			return View(dashboard);
		}
	}
}