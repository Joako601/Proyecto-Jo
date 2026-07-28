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

		public IActionResult Index(string? categoria, int pagina = 1)
		{
			const int tamanoPagina = 8;

			var menu = _productoService.ObtenerMenu();

			var resultado = string.IsNullOrEmpty(categoria)
				? menu
				: menu.Where(i => i.Categoria == categoria).ToList();

			var totalItems = resultado.Count;
			var totalPaginas = (int)Math.Ceiling(totalItems / (double)tamanoPagina);
			if (totalPaginas < 1) totalPaginas = 1;
			if (pagina < 1) pagina = 1;
			if (pagina > totalPaginas) pagina = totalPaginas;

			var paginaActual = resultado
				.Skip((pagina - 1) * tamanoPagina)
				.Take(tamanoPagina)
				.ToList();

			var promosVigentes = _promocionService.ObtenerVigentes().ToList();

			var viewModels = paginaActual.Select(i => new MenuItemViewModel
			{
				Platillo = i,
				PrecioFinal = _promocionService.CalcularPrecioFinal(i, promosVigentes)
			}).ToList();

			ViewBag.Categorias = menu.Select(i => i.Categoria).Distinct().ToList();
			ViewBag.CategoriaActual = categoria;
			ViewBag.PromocionesGenerales = promosVigentes.Where(p => p.ItemIds == null || p.ItemIds.Count == 0).ToList();
			ViewBag.PaginaActual = pagina;
			ViewBag.TotalPaginas = totalPaginas;

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
	}
}