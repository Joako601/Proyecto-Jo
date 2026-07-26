using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoJo.Application.Ports.In;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Web.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(AuthenticationSchemes = "JoCookieAuth")]
	public class OpinionesController : Controller
	{
		private readonly IOpinionService _opinionService;
		private readonly IProductoService _productoService;

		public OpinionesController(IOpinionService opinionService, IProductoService productoService)
		{
			_opinionService = opinionService;
			_productoService = productoService;
		}

		// GET: /Admin/Opiniones
		public IActionResult Index()
		{
			var opiniones = _opinionService.ObtenerTodas();
			return View(opiniones);
		}

		// GET: /Admin/Opiniones/Agregar
		public IActionResult Agregar()
		{
			ViewBag.Platillos = _productoService.ObtenerTodos().ToList();
			return View(new OpinionCliente());
		}

		// POST: /Admin/Opiniones/Agregar
		[HttpPost]
		public IActionResult Agregar(OpinionCliente opinion)
		{
			ValidarOpinion(opinion);

			if (!ModelState.IsValid)
			{
				ViewBag.Platillos = _productoService.ObtenerTodos().ToList();
				return View(opinion);
			}

			_opinionService.Agregar(opinion, User.Identity?.Name ?? "Desconocido");
			return RedirectToAction("Index");
		}

		// GET: /Admin/Opiniones/Editar/5
		public IActionResult Editar(int id)
		{
			var opinion = _opinionService.ObtenerPorId(id);
			if (opinion is null) return NotFound();

			ViewBag.Platillos = _productoService.ObtenerTodos().ToList();
			return View(opinion);
		}

		// POST: /Admin/Opiniones/Editar
		[HttpPost]
		public IActionResult Editar(OpinionCliente opinion)
		{
			ValidarOpinion(opinion);

			if (!ModelState.IsValid)
			{
				ViewBag.Platillos = _productoService.ObtenerTodos().ToList();
				return View(opinion);
			}

			var actualizado = _opinionService.Editar(opinion, User.Identity?.Name ?? "Desconocido");
			if (!actualizado) return NotFound();
			return RedirectToAction("Index");
		}

		// POST: /Admin/Opiniones/Eliminar/5
		[HttpPost]
		public IActionResult Eliminar(int id)
		{
			var eliminado = _opinionService.Eliminar(id, User.Identity?.Name ?? "Desconocido");
			if (!eliminado) return NotFound();
			return RedirectToAction("Index");
		}

		private void ValidarOpinion(OpinionCliente opinion)
		{
			if (string.IsNullOrWhiteSpace(opinion.Comentario))
				ModelState.AddModelError(nameof(OpinionCliente.Comentario), "Escribí el comentario del cliente.");

			if (opinion.Calificacion < 1 || opinion.Calificacion > 5)
				ModelState.AddModelError(nameof(OpinionCliente.Calificacion), "La calificación debe estar entre 1 y 5 estrellas.");

			if ((opinion.Calificacion * 2) % 1 != 0)
				ModelState.AddModelError(nameof(OpinionCliente.Calificacion), "La calificación solo admite pasos de media estrella.");

			if (opinion.ItemId is not null && _productoService.ObtenerPorId(opinion.ItemId.Value) is null)
				ModelState.AddModelError(nameof(OpinionCliente.ItemId), "El platillo seleccionado ya no existe.");
		}
	}
}