using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoJo.Application.Ports.In;
using ProyectoJo.Domain.Entities;
using ProyectoJo.Web.Authorization;

namespace ProyectoJo.Web.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(AuthenticationSchemes = "JoCookieAuth")]
	[RequiereArea("Insumos")]
	public class InsumosController : Controller
	{
		private readonly IInsumoService _insumoService;
		private readonly IProductoService _productoService;

		public InsumosController(IInsumoService insumoService, IProductoService productoService)
		{
			_insumoService = insumoService;
			_productoService = productoService;
		}

		// GET: /Admin/Insumos
		public IActionResult Index()
		{
			var insumos = _insumoService.ObtenerTodos()
				.OrderBy(i => i.Nombre)
				.ToList();
			return View(insumos);
		}

		// GET: /Admin/Insumos/Crear
		public IActionResult Crear() => View(new Insumo());

		// POST: /Admin/Insumos/Crear
		[HttpPost]
		public IActionResult Crear(Insumo insumo)
		{
			if (!ModelState.IsValid) return View(insumo);
			insumo.DescartarId();
			_insumoService.Agregar(insumo, User.Identity?.Name ?? "Desconocido");
			return RedirectToAction(nameof(Index));
		}

		// GET: /Admin/Insumos/Editar/5
		public IActionResult Editar(int id)
		{
			var insumo = _insumoService.ObtenerPorId(id);
			if (insumo is null) return NotFound();
			return View(insumo);
		}

		// POST: /Admin/Insumos/Editar/5
		[HttpPost]
		public IActionResult Editar(Insumo insumo)
		{
			if (!ModelState.IsValid) return View(insumo);
			var actualizado = _insumoService.Editar(insumo, User.Identity?.Name ?? "Desconocido");
			if (!actualizado) return NotFound();
			return RedirectToAction(nameof(Index));
		}

		// POST: /Admin/Insumos/Reponer
		[HttpPost]
		public async Task<IActionResult> Reponer(int id, decimal cantidad)
		{
			var resultado = await _insumoService.ReponerAsync(id, cantidad, User.Identity?.Name ?? "Desconocido");
			if (resultado != ResultadoReponerInsumo.Exitoso)
			{
				TempData["Error"] = resultado switch
				{
					ResultadoReponerInsumo.CantidadInvalida => "La cantidad a reponer debe ser mayor a 0.",
					ResultadoReponerInsumo.InsumoNoEncontrado => "El insumo indicado no existe.",
					ResultadoReponerInsumo.ConflictoDeConcurrencia => "El insumo fue modificado por otro proceso. Intentalo de nuevo.",
					_ => "No se pudo reponer el stock del insumo indicado."
				};
			}
			return RedirectToAction(nameof(Index));
		}

		// POST: /Admin/Insumos/Eliminar/5
		[HttpPost]
		public IActionResult Eliminar(int id)
		{
			var (exito, error) = _insumoService.Eliminar(id, User.Identity?.Name ?? "Desconocido");
			if (!exito) TempData["Error"] = error;
			return RedirectToAction(nameof(Index));
		}

		// POST: /Admin/Insumos/SincronizarDesdeMenu
		[HttpPost]
		public IActionResult SincronizarDesdeMenu()
		{
			var menu = _productoService.ObtenerMenu();
			var nuevos = _insumoService.SincronizarDesdeMenu(menu, User.Identity?.Name ?? "Desconocido");

			TempData["MensajeSincronizacion"] = nuevos > 0
				? $"Se crearon {nuevos} insumo(s) nuevo(s) a partir del menú."
				: "El menú no tiene ingredientes nuevos para sincronizar.";

			return RedirectToAction(nameof(Index));
		}
	}
}