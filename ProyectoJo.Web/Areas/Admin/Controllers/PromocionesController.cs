using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using ProyectoJo.Application.Ports.In;
using ProyectoJo.Domain.Entities;
using ProyectoJo.Web.Authorization;

namespace ProyectoJo.Web.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(AuthenticationSchemes = "JoCookieAuth")]
	[RequiereArea("Promociones")]
	public class PromocionesController : Controller
	{
		private readonly IPromocionService _promocionService;
		private readonly IProductoService _productoService;
		private readonly IWebHostEnvironment _entorno;

		private static readonly string[] ExtensionesPermitidas = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
		private const long TamanoMaximoBytes = 5 * 1024 * 1024; // 5 MB

		public PromocionesController(
			IPromocionService promocionService,
			IProductoService productoService,
			IWebHostEnvironment entorno)
		{
			_promocionService = promocionService;
			_productoService = productoService;
			_entorno = entorno;
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

			promocion.Id = 0;
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
			var cambiado = _promocionService.ToggleActiva(id, User.Identity?.Name ?? "Desconocido");
			if (!cambiado) return NotFound();

			return RedirectToAction("Index");
		}

		// POST: /Admin/Promociones/ActualizarFecha
		[HttpPost]
		public IActionResult ActualizarFecha(int id, DateTime? fechaInicio, DateTime? fechaFin)
		{
			try
			{
				var actualizado = _promocionService.ActualizarFecha(id, fechaInicio, fechaFin, User.Identity?.Name ?? "Desconocido");
				if (!actualizado) return NotFound();
				return RedirectToAction("Index");
			}
			catch (InvalidOperationException ex)
			{
				TempData["Error"] = ex.Message;
				return RedirectToAction("Index");
			}
		}

		// POST: /Admin/Promociones/HacerPermanente/5
		[HttpPost]
		public IActionResult HacerPermanente(int id)
		{
			var actualizado = _promocionService.HacerPermanente(id, User.Identity?.Name ?? "Desconocido");
			if (!actualizado) return NotFound();
			return RedirectToAction("Index");
		}

		// POST: /Admin/Promociones/SubirImagen
		[HttpPost]
		public async Task<IActionResult> SubirImagen(IFormFile archivo)
		{
			if (archivo == null || archivo.Length == 0)
				return BadRequest(new { error = "No se recibió ningún archivo." });

			var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
			if (!ExtensionesPermitidas.Contains(extension))
				return BadRequest(new { error = "Formato no permitido. Usá JPG, PNG, WEBP o GIF." });

			if (archivo.Length > TamanoMaximoBytes)
				return BadRequest(new { error = "La imagen no puede superar los 5 MB." });

			if (!await EsImagenValidaAsync(archivo))
				return BadRequest(new { error = "El archivo no es una imagen válida." });

			var carpeta = Path.Combine(_entorno.WebRootPath, "uploads", "promociones");
			Directory.CreateDirectory(carpeta);

			var nombreArchivo = $"{Guid.NewGuid()}{extension}";
			var rutaFisica = Path.Combine(carpeta, nombreArchivo);

			using (var stream = new FileStream(rutaFisica, FileMode.Create))
			{
				await archivo.CopyToAsync(stream);
			}

			var urlRelativa = $"/uploads/promociones/{nombreArchivo}";
			return Json(new { url = urlRelativa });
		}

		private static async Task<bool> EsImagenValidaAsync(IFormFile archivo)
		{
			var encabezado = new byte[12];
			await using var stream = archivo.OpenReadStream();

			var leidos = 0;
			while (leidos < encabezado.Length)
			{
				var bloque = await stream.ReadAsync(encabezado.AsMemory(leidos, encabezado.Length - leidos));
				if (bloque == 0) break;
				leidos += bloque;
			}

			if (leidos < 4) return false;

			if (encabezado[0] == 0xFF && encabezado[1] == 0xD8 && encabezado[2] == 0xFF)
				return true; // JPEG

			if (encabezado[0] == 0x89 && encabezado[1] == 0x50 && encabezado[2] == 0x4E && encabezado[3] == 0x47)
				return true; // PNG

			if (encabezado[0] == 0x47 && encabezado[1] == 0x49 && encabezado[2] == 0x46 && encabezado[3] == 0x38)
				return true; // GIF

			if (leidos == 12 &&
				encabezado[0] == 0x52 && encabezado[1] == 0x49 && encabezado[2] == 0x46 && encabezado[3] == 0x46 &&
				encabezado[8] == 0x57 && encabezado[9] == 0x45 && encabezado[10] == 0x42 && encabezado[11] == 0x50)
				return true; // WEBP

			return false;
		}
	}
}