using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoJo.Application.Ports.In;
using ProyectoJo.Domain.Entities;
using ProyectoJo.Web.Authorization;

namespace ProyectoJo.Web.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(AuthenticationSchemes = "JoCookieAuth")]
	[RequiereArea("Finanzas")]
	public class FinanzasController : Controller
	{
		private readonly IFinanzaService _finanzaService;

		public FinanzasController(IFinanzaService finanzaService)
		{
			_finanzaService = finanzaService;
		}

		private List<string> ObtenerCategorias() =>
			_finanzaService.ObtenerTodos()
				.Select(f => f.Categoria)
				.Where(c => !string.IsNullOrWhiteSpace(c))
				.Distinct(StringComparer.OrdinalIgnoreCase)
				.OrderBy(c => c)
				.ToList();

		// GET: /Admin/Finanzas
		public IActionResult Index(int? mes, int? anio, int pagina = 1)
		{
			var hoy = DateTime.Today;
			var mesActual = mes ?? hoy.Month;
			var anioActual = anio ?? hoy.Year;
			const int porPagina = 12;

			if (pagina < 1) pagina = 1;

			var (movimientos, total) = _finanzaService.ObtenerPaginado(mesActual, anioActual, pagina, porPagina);

			var resumen = _finanzaService.ObtenerResumenDelDia(hoy);
			ViewBag.Resumen = resumen;
			ViewBag.Mes = mesActual;
			ViewBag.Anio = anioActual;
			ViewBag.PaginaActual = pagina;
			ViewBag.TotalPaginas = (int)Math.Ceiling(total / (double)porPagina);
			ViewBag.TotalMovimientos = total;

			return View(movimientos);
		}

		// GET: /Admin/Finanzas/Registrar
		public IActionResult Registrar()
		{
			ViewBag.Categorias = ObtenerCategorias();
			return View();
		}

		// POST: /Admin/Finanzas/Registrar
		[HttpPost]
		public IActionResult Registrar(Finanza finanza)
		{
			if (!ModelState.IsValid)
			{
				ViewBag.Categorias = ObtenerCategorias();
				return View(finanza);
			}
			finanza.DescartarId();
			_finanzaService.RegistrarMovimiento(finanza, User.Identity?.Name ?? "Desconocido");
			return RedirectToAction("Index");
		}

		// GET: /Admin/Finanzas/Editar/5
		public IActionResult Editar(int id)
		{
			var finanza = _finanzaService.ObtenerPorId(id);
			if (finanza == null) return NotFound();
			ViewBag.Categorias = ObtenerCategorias();
			return View(finanza);
		}

		// POST: /Admin/Finanzas/Editar
		[HttpPost]
		public IActionResult Editar(Finanza finanza)
		{
			if (!ModelState.IsValid)
			{
				ViewBag.Categorias = ObtenerCategorias();
				return View(finanza);
			}
			var actualizado = _finanzaService.Editar(finanza, User.Identity?.Name ?? "Desconocido");
			if (!actualizado) return NotFound();
			return RedirectToAction("Index");
		}

		// POST: /Admin/Finanzas/Eliminar/5
		[HttpPost]
		public IActionResult Eliminar(int id)
		{
			var eliminado = _finanzaService.Eliminar(id, User.Identity?.Name ?? "Desconocido");
			if (!eliminado) return NotFound();
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