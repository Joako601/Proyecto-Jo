using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoJo.Application.Ports.In;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Web.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(AuthenticationSchemes = "JoCookieAuth")]
	public class MenuController : Controller
	{
		private readonly IProductoService _productoService;

		public MenuController(IProductoService productoService)
		{
			_productoService = productoService;
		}

		// GET: /Admin/Menu
		public IActionResult Index()
		{
			var menu = _productoService.ObtenerTodos().ToList();
			return View(menu);
		}

		// GET: /Admin/Menu/Agregar
		public IActionResult Agregar() => View();

		// POST: /Admin/Menu/Agregar
		[HttpPost]
		public IActionResult Agregar(Item item)
		{
			if (!ModelState.IsValid) return View(item);
			_productoService.AgregarItem(item);
			return RedirectToAction("Index");
		}

		// GET: /Admin/Menu/Editar/5
		public IActionResult Editar(int id)
		{
			var item = _productoService.ObtenerPorId(id);
			return item == null ? NotFound() : View(item);
		}

		// POST: /Admin/Menu/Editar
		[HttpPost]
		public IActionResult Editar(Item item)
		{
			if (!ModelState.IsValid) return View(item);
			_productoService.EditarItem(item);
			return RedirectToAction("Index");
		}

		// POST: /Admin/Menu/Eliminar/5
		[HttpPost]
		public IActionResult Eliminar(int id)
		{
			_productoService.Eliminar(id);
			return RedirectToAction("Index");
		}
	}
}