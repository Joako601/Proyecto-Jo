using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoJo.Application.Ports.In;
using ProyectoJo.Web.Authorization;

namespace ProyectoJo.Web.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(AuthenticationSchemes = "JoCookieAuth")]
	[RequiereArea("MapaCalor")]
	public class MapaCalorController : Controller
	{
		private readonly IReporteService _reporteService;

		public MapaCalorController(IReporteService reporteService)
		{
			_reporteService = reporteService;
		}


		// GET: /Admin/MapaCalor?fecha=2026-06-25&semanaHistorico=true&semanaOffset=0&anioMeses=2026&mesDetalle=6
		public async Task<IActionResult> Index(
			DateTime? fecha,
			bool semanaHistorico = true,
			int semanaOffset = 0,
			int? anioMeses = null,
			int? mesDetalle = null)
		{
			var resumen = await _reporteService.ObtenerMapaCalorAsync(
				fecha, null, semanaHistorico, semanaOffset, anioMeses, mesDetalle);

			return View(resumen);
		}
	}
}