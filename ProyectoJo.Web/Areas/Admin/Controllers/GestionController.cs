using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoJo.Application.Ports.In;
using ProyectoJo.Web.Models;

namespace ProyectoJo.Web.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(AuthenticationSchemes = "JoCookieAuth")]
	public class GestionController : Controller
	{
		private readonly IProductoService _productoService;
		private readonly IFinanzaService _finanzaService;
		private readonly IOpinionService _opinionService;

		public GestionController(IProductoService productoService, IFinanzaService finanzaService, IOpinionService opinionService)
		{
			_productoService = productoService;
			_finanzaService = finanzaService;
			_opinionService = opinionService;
		}

		public IActionResult Index()
		{
			var resumenHoy = _finanzaService.ObtenerResumenDelDia(DateTime.Today);

			var vm = new DashboardViewModel
			{
				TotalPlatillos = _productoService.ObtenerMenu().Count,
				VentasHoy = resumenHoy.TotalIngresos,
				PendientesHoy = 0,
				TotalOpiniones = _opinionService.ContarTotal()
			};

			return View(vm);
		}
	}
}