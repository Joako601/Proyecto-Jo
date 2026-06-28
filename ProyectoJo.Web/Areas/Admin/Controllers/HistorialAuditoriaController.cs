using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoJo.Application.Ports.In;

namespace ProyectoJo.Web.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(AuthenticationSchemes = "JoCookieAuth")]
	public class HistorialAuditoriaController : Controller
	{
		private readonly IAuditoriaService _auditoriaService;

		public HistorialAuditoriaController(IAuditoriaService auditoriaService)
		{
			_auditoriaService = auditoriaService;
		}

		public IActionResult Index(string? modulo, DateTime? desde, DateTime? hasta)
		{
			var historial = _auditoriaService.ObtenerHistorial(modulo, desde, hasta);
			return View(historial);
		}
	}
}
