using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoJo.Application.Ports.In;

namespace ProyectoJo.Web.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(AuthenticationSchemes = "JoCookieAuth", Roles = "SuperAdmin")]
	public class AdministradoresController : Controller
	{
		private readonly IAdministradorService _administradorService;

		public AdministradoresController(IAdministradorService administradorService)
		{
			_administradorService = administradorService;
		}

		public async Task<IActionResult> Index()
		{
			var administradores = await _administradorService.ObtenerTodosAsync();
			return View(administradores.OrderBy(a => a.Usuario).ToList());
		}

		[HttpPost]
		public async Task<IActionResult> Crear(string usuario, string contrasena, bool general, List<string>? areas, string? claveSupervisor)
		{
			var areasFinal = general ? new List<string>() : (areas ?? new List<string>());
			var (exito, error) = await _administradorService.CrearAsync(usuario, contrasena, areasFinal, claveSupervisor);
			TempData["Error"] = exito ? null : error;
			TempData["Exito"] = exito ? "Administrador creado correctamente." : null;
			return RedirectToAction("Index");
		}

		[HttpPost]
		public async Task<IActionResult> Editar(int id, string usuario, bool activo, string? nuevaContrasena, bool general, List<string>? areas, string? nuevaClaveSupervisor)
		{
			var areasFinal = general ? new List<string>() : (areas ?? new List<string>());
			var (exito, error) = await _administradorService.EditarAsync(id, usuario, activo, nuevaContrasena, areasFinal, nuevaClaveSupervisor);
			TempData["Error"] = exito ? null : error;
			TempData["Exito"] = exito ? "Administrador actualizado correctamente." : null;
			return RedirectToAction("Index");
		}

		[HttpPost]
		public async Task<IActionResult> Eliminar(int id)
		{
			var exito = await _administradorService.EliminarAsync(id);
			TempData["Error"] = exito ? null : "No se pudo eliminar el administrador.";
			return RedirectToAction("Index");
		}
	}
}