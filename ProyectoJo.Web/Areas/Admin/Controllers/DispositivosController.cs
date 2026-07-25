using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoJo.Application.Ports.In;

namespace ProyectoJo.Web.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(AuthenticationSchemes = "JoCookieAuth")]
	public class DispositivosController : Controller
	{
		private readonly IDispositivoService _dispositivoService;

		public DispositivosController(IDispositivoService dispositivoService)
		{
			_dispositivoService = dispositivoService;
		}

		public async Task<IActionResult> Index()
		{
			var dispositivos = await _dispositivoService.ObtenerTodosAsync();
			return View(dispositivos.OrderByDescending(d => d.FechaRegistro).ToList());
		}
	}
}