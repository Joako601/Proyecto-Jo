using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoJo.Application.Ports.In;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Web.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(AuthenticationSchemes = "JoCookieAuth")]
	public class RecetarioController : Controller
	{
		private readonly IRecetaService _recetaService;
		private readonly IProductoService _productoService;

		public RecetarioController(IRecetaService recetaService, IProductoService productoService)
		{
			_recetaService = recetaService;
			_productoService = productoService;
		}



		// GET: /Admin/Recetario
		public IActionResult Index()
		{
			var rendimientos = _recetaService.ObtenerRendimientoDeTodas();
			return View(rendimientos);
		}

		// GET: /Admin/Recetario/Agregar
		public IActionResult Agregar()
		{
			var itemIdsConReceta = _recetaService.ObtenerTodas()
				.Select(r => r.ItemId)
				.ToHashSet();

			ViewBag.Platillos = _productoService.ObtenerTodos()
				.Where(i => !itemIdsConReceta.Contains(i.Id))
				.ToList();
			return View(new Receta { Ingredientes = new List<IngredienteReceta> { new() } });
		}



		// POST: /Admin/Recetario/Agregar
		[HttpPost]
		public IActionResult Agregar(Receta receta)
		{
			receta.Ingredientes = (receta.Ingredientes ?? new())
				.Where(i => !string.IsNullOrWhiteSpace(i.Nombre))
				.ToList();

			var item = _productoService.ObtenerPorId(receta.ItemId);
			if (item is not null)
			{
				receta.NombreReceta = item.Platillo;
			}

			if (!ModelState.IsValid || item is null || !receta.Ingredientes.Any())
			{
				ViewBag.Platillos = _productoService.ObtenerTodos().ToList();
				if (item is null)
					ModelState.AddModelError(nameof(Receta.ItemId), "Seleccioná un platillo del menú.");
				if (!receta.Ingredientes.Any())
					ModelState.AddModelError("", "Agregá al menos un ingrediente.");
				return View(receta);
			}

			_recetaService.Agregar(receta, User.Identity?.Name ?? "Desconocido");
			return RedirectToAction("Index");
		}

		// GET: /Admin/Recetario/Editar/5
		public IActionResult Editar(int id)
		{
			var receta = _recetaService.ObtenerPorId(id);
			if (receta is null) return NotFound();

			ViewBag.Platillos = _productoService.ObtenerTodos().ToList();
			return View(receta);
		}

		// POST: /Admin/Recetario/Editar
		[HttpPost]
		public IActionResult Editar(Receta receta)
		{
			receta.Ingredientes = (receta.Ingredientes ?? new())
				.Where(i => !string.IsNullOrWhiteSpace(i.Nombre))
				.ToList();

			var item = _productoService.ObtenerPorId(receta.ItemId);
			if (item is not null)
			{
				receta.NombreReceta = item.Platillo;
			}

			if (!ModelState.IsValid || item is null || !receta.Ingredientes.Any())
			{
				ViewBag.Platillos = _productoService.ObtenerTodos().ToList();
				if (item is null)
					ModelState.AddModelError(nameof(Receta.ItemId), "Seleccioná un platillo del menú.");
				if (!receta.Ingredientes.Any())
					ModelState.AddModelError("", "Agregá al menos un ingrediente.");
				return View(receta);
			}

			var actualizado = _recetaService.Editar(receta, User.Identity?.Name ?? "Desconocido");
			if (!actualizado) return NotFound();
			return RedirectToAction("Index");
		}

		// POST: /Admin/Recetario/Eliminar/5
		[HttpPost]
		public IActionResult Eliminar(int id)
		{
			var eliminado = _recetaService.Eliminar(id, User.Identity?.Name ?? "Desconocido");
			if (!eliminado) return NotFound();
			return RedirectToAction("Index");
		}
	}
}