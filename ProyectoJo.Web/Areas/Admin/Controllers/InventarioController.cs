using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoJo.Application.Ports.In;

namespace ProyectoJo.Web.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(AuthenticationSchemes = "JoCookieAuth")]
	public class InventarioController : Controller
	{
		private readonly IProductoService _productoService;

		public InventarioController(IProductoService productoService)
		{
			_productoService = productoService;
		}

		// GET: /Admin/Inventario
		public IActionResult Index()
		{
			var menu = _productoService.ObtenerTodos();
			return View(menu);
		}

		// POST: /Admin/Inventario/ToggleActivo/5
		[HttpPost]
		public IActionResult ToggleActivo(int id, string origen = "Inventario")
		{
			var cambiado = _productoService.ToggleActivo(id, User.Identity?.Name ?? "Desconocido");
			if (!cambiado) return NotFound();

			return origen == "Menu"
				? RedirectToAction("Index", "Menu")
				: RedirectToAction("Index");
		}

		// POST: /Admin/Inventario/ToggleAgotado/5
		[HttpPost]
		public IActionResult ToggleAgotado(int id, string origen = "Inventario")
		{
			var cambiado = _productoService.ToggleAgotado(id, User.Identity?.Name ?? "Desconocido");
			if (!cambiado) return NotFound();

			return origen == "Menu"
				? RedirectToAction("Index", "Menu")
				: RedirectToAction("Index");
		}
	}
}