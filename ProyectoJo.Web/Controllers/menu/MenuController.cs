using Microsoft.AspNetCore.Mvc;
using ProyectoJo.Domain.Entities;
using ProyectoJo.Application.Ports.In;
using ProyectoJo.Web.Models;

namespace ProyectoJo.Web.Controllers
{
	public class MenuController : Controller
	{
		private readonly IProductoService _productoService;
		private readonly IPromocionService _promocionService;

		public MenuController(IProductoService productoService, IPromocionService promocionService)
		{
			_productoService = productoService;
			_promocionService = promocionService;
		}

		public IActionResult Index(string? categoria)
		{
			var menu = _productoService.ObtenerMenu();

			var resultado = string.IsNullOrEmpty(categoria)
				? menu
				: menu.Where(i => i.Categoria == categoria).ToList();

			var viewModels = resultado.Select(i => new MenuItemViewModel
			{
				Platillo = i,
				PrecioFinal = _promocionService.CalcularPrecioFinal(i)
			}).ToList();

			ViewBag.Categorias = menu.Select(i => i.Categoria).Distinct().ToList();
			ViewBag.CategoriaActual = categoria;
			ViewBag.PromocionesGenerales = _promocionService.ObtenerVigentesGenerales().ToList();

			return View(viewModels);
		}

		public IActionResult Detalle(int id)
		{
			var menu = _productoService.ObtenerMenu();
			var platillo = menu.FirstOrDefault(i => i.Id == id);

			if (platillo == null) return NotFound();

			var vm = new MenuItemViewModel
			{
				Platillo = platillo,
				PrecioFinal = _promocionService.CalcularPrecioFinal(platillo)
			};

			return View(vm);
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

			_productoService.AgregarItem(nuevo, "Cliente público");

			return RedirectToAction("Index");
		}
	}
}