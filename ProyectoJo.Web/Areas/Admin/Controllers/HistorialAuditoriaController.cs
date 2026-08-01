using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoJo.Application.Ports.In;
using ProyectoJo.Web.Authorization;

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
			if (pagina < 1) pagina = 1;

			var (historial, total) = _auditoriaService.ObtenerHistorialPaginado(modulo, desde, hasta, pagina, porPagina);

			ViewBag.PaginaActual = pagina;
			ViewBag.TotalPaginas = (int)Math.Ceiling(total / (double)porPagina);
			ViewBag.TotalRegistros = total;

			return View(historial);
		}
	}
}
