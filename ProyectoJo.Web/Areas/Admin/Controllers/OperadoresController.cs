using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoJo.Application.Ports.In;
using ProyectoJo.Domain.Entities;
using ProyectoJo.Web.Authorization;

namespace ProyectoJo.Web.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(AuthenticationSchemes = "JoCookieAuth", Roles = "SuperAdmin,Administrador")]
	[RequiereArea("Operadores")]
	public class OperadoresController : Controller
	{
		private readonly IEmpleadoService _empleadoService;

		public OperadoresController(IEmpleadoService empleadoService)
		{
			_empleadoService = empleadoService;
		}

		public async Task<IActionResult> Index()
		{
			var operadores = await _empleadoService.ObtenerTodosAsync();
			return View(operadores.OrderBy(e => e.Nombre).ToList());
		}

		[HttpPost]
		public async Task<IActionResult> Crear(string nombre, string pin, RolEmpleado rol)
		{
			var (exito, error) = await _empleadoService.CrearAsync(nombre, pin, rol);
			TempData["Error"] = exito ? null : error;
			TempData["Exito"] = exito ? "Operador creado correctamente." : null;
			return RedirectToAction("Index");
		}

		[HttpPost]
		public async Task<IActionResult> Editar(int id, string nombre, bool activo, RolEmpleado rol, string? nuevoPin)
		{
			var (exito, error) = await _empleadoService.EditarAsync(id, nombre, activo, rol, nuevoPin);
			TempData["Error"] = exito ? null : error;
			TempData["Exito"] = exito ? "Operador actualizado correctamente." : null;
			return RedirectToAction("Index");
		}

		[HttpPost]
		public async Task<IActionResult> Eliminar(int id)
		{
			var exito = await _empleadoService.EliminarAsync(id);
			TempData["Error"] = exito ? null : "No se pudo eliminar el operador.";
			return RedirectToAction("Index");
		}
	}
}