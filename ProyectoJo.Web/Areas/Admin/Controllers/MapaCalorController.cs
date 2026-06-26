using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoJo.Application.Ports.In;

namespace ProyectoJo.Web.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(AuthenticationSchemes = "JoCookieAuth")]
	public class MapaCalorController : Controller
	{
		private readonly IPedidoService _pedidoService;

		public MapaCalorController(IPedidoService pedidoService)
		{
			_pedidoService = pedidoService;
		}


		// GET: /Admin/MapaCalor?fecha=2026-06-25&semanaHistorico=true&semanaOffset=0&anioMeses=2026&mesDetalle=6
		public async Task<IActionResult> Index(
			DateTime? fecha,
			bool semanaHistorico = true,
			int semanaOffset = 0,
			int? anioMeses = null,
			int? mesDetalle = null)
		{
			var resumen = await _pedidoService.ObtenerMapaCalorAsync(
				fecha, null, semanaHistorico, semanaOffset, anioMeses, mesDetalle);

			return View(resumen);
		}
	}
}