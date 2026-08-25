using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoJo.Application.Ports.In;
using ProyectoJo.Web.Authorization;
using ProyectoJo.Web.Helpers;

namespace ProyectoJo.Web.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(AuthenticationSchemes = "JoCookieAuth")]
	[RequiereArea("Auditoria")]
	public class HistorialAuditoriaController : Controller
	{
		private readonly IAuditoriaService _auditoriaService;

		public HistorialAuditoriaController(IAuditoriaService auditoriaService)
		{
			_auditoriaService = auditoriaService;
		}

		public IActionResult Index(string? modulo, DateTime? desde, DateTime? hasta, int pagina = 1)
		{
			const int porPagina = 15;
			pagina = PaginacionHelper.NormalizarPaginaMinima(pagina);

			var (historial, total) = _auditoriaService.ObtenerHistorialPaginado(modulo, desde, hasta, pagina, porPagina);

			ViewBag.PaginaActual = pagina;
			ViewBag.TotalPaginas = PaginacionHelper.CalcularTotalPaginas(total, porPagina);
			ViewBag.TotalRegistros = total;

			return View(historial);
		}
	}
}
