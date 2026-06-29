using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoJo.Application.Ports.In;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Web.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(AuthenticationSchemes = "JoCookieAuth")]
	public class PromocionesController : Controller
	{
		private readonly IPromocionService _promocionService;
		private readonly IProductoService _productoService;

		public PromocionesController(IPromocionService promocionService, IProductoService productoService)
		{
			_promocionService = promocionService;
			_productoService = productoService;
		}

		// GET: /Admin/Promociones
		public IActionResult Index()
		{
			var promociones = _promocionService.ObtenerTodas().ToList();
			ViewBag.Vigentes = promociones.Where(p => _promocionService.EstaVigente(p))
				.Select(p => p.Id).ToHashSet();
			return View(promociones);
		}

		// GET: /Admin/Promociones/Agregar
		public IActionResult Agregar()
		{
			ViewBag.Platillos = _productoService.ObtenerTodos().ToList();
			return View(new Promocion());
		}

		// POST: /Admin/Promociones/Agregar
		[HttpPost]
		public IActionResult Agregar(Promocion promocion)
		{
			if (!ModelState.IsValid)
			{
				ViewBag.Platillos = _productoService.ObtenerTodos().ToList();
				return View(promocion);
			}

			_promocionService.Agregar(promocion, User.Identity?.Name ?? "Desconocido");
			return RedirectToAction("Index");
		}

		// GET: /Admin/Promociones/Editar/5
		public IActionResult Editar(int id)
		{
			var promo = _promocionService.ObtenerPorId(id);
			if (promo == null) return NotFound();

			ViewBag.Platillos = _productoService.ObtenerTodos().ToList();
			return View(promo);
		}

		// POST: /Admin/Promociones/Editar
		[HttpPost]
		public IActionResult Editar(Promocion promocion)
		{
			if (!ModelState.IsValid)
			{
				ViewBag.Platillos = _productoService.ObtenerTodos().ToList();
				return View(promocion);
			}

			var actualizado = _promocionService.Editar(promocion, User.Identity?.Name ?? "Desconocido");
			if (!actualizado) return NotFound();
			return RedirectToAction("Index");
		}

		// POST: /Admin/Promociones/Eliminar/5
		[HttpPost]
		public IActionResult Eliminar(int id)
		{
			var eliminado = _promocionService.Eliminar(id, User.Identity?.Name ?? "Desconocido");
			if (!eliminado) return NotFound();
			return RedirectToAction("Index");
		}

		// POST: /Admin/Promociones/ToggleActiva/5
		[HttpPost]
		public IActionResult ToggleActiva(int id)
		{
			_promocionService.ToggleActiva(id, User.Identity?.Name ?? "Desconocido");
			return RedirectToAction("Index");
		}
	}
}