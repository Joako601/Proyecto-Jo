using Microsoft.AspNetCore.Mvc;
using ProyectoJo.Domain.Entities;
using ProyectoJo.Application.Ports.In;

namespace ProyectoJo.Web.Controllers
{
	public class MenuController : Controller
	{
		private readonly IProductoService _productoService;

		public MenuController(IProductoService productoService)
		{
			_productoService = productoService;
		}

		public IActionResult Index(string? categoria)
		{
			var menu = _productoService.ObtenerMenu();

			var resultado = string.IsNullOrEmpty(categoria)
				? menu
				: menu.Where(i => i.Categoria == categoria).ToList();

			ViewBag.Categorias = menu.Select(i => i.Categoria).Distinct().ToList();
			ViewBag.CategoriaActual = categoria;

			return View(resultado);
		}

		public IActionResult Detalle(int id)
		{
			var menu = _productoService.ObtenerMenu();
			var platillo = menu.FirstOrDefault(i => i.Id == id);

			return platillo == null ? NotFound() : View(platillo);
		}

		// GET: /Catalogo/Agregar
		public IActionResult Agregar() => View();

		// POST: /Catalogo/Agregar
		[HttpPost]
		public IActionResult Agregar(Item nuevo)
		{
			if (!ModelState.IsValid)
			{
				return View(nuevo);
			}

			_productoService.AgregarItem(nuevo);

			return RedirectToAction("Index");
		}
	}
}