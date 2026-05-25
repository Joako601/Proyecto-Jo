using Microsoft.AspNetCore.Mvc;
using Proyecto_Jo_.Models; // Importa tu modelo Item corregido
using Proyecto_Jo_.Data;   // Importa tu JsonProductService

namespace Proyecto_Jo_.Controllers
{
	public class CatalogoController : Controller
	{
		private readonly JsonProductService _productService;

		// El constructor recibe el servicio de JSON gracias a la línea que pusimos en Program.cs
		public CatalogoController(JsonProductService productService)
		{
			_productService = productService;
		}

		// GET: /Catalogo/Index
		public IActionResult Index(string? categoria)
		{
			// Leemos los platillos directamente desde el archivo JSON
			var menu = _productService.ObtenerMenu();

			var resultado = string.IsNullOrEmpty(categoria)
				? menu
				: menu.Where(i => i.Categoria == categoria).ToList();

			ViewBag.Categorias = menu.Select(i => i.Categoria).Distinct().ToList();
			ViewBag.CategoriaActual = categoria;

			return View(resultado);
		}

		// GET: /Catalogo/Detalle/{id}
		public IActionResult Detalle(int id)
		{
			var menu = _productService.ObtenerMenu();
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

			var menu = _productService.ObtenerMenu();

			// Autoincrementa el ID de forma dinámica basándose en lo que hay en el JSON
			nuevo.Id = menu.Count > 0 ? menu.Max(i => i.Id) + 1 : 1;

			menu.Add(nuevo);
			_productService.GuardarMenu(menu); // Guarda el nuevo platillo de forma permanente en el JSON

			return RedirectToAction("Index");
		}
	}
}