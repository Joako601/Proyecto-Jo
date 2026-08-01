using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoJo.Application.Ports.In;
using ProyectoJo.Domain.Entities;
using ProyectoJo.Infrastructure.Auth;
using ProyectoJo.Web.Areas.Admin.Models;
using ProyectoJo.Web.Authorization;

namespace ProyectoJo.Web.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(AuthenticationSchemes = "JoCookieAuth", Roles = "SuperAdmin,Administrador")]
	public class UsuariosController : Controller
	{
		private readonly IAdministradorService _administradorService;
		private readonly IEmpleadoService _empleadoService;

		public UsuariosController(IAdministradorService administradorService, IEmpleadoService empleadoService)
		{
			_administradorService = administradorService;
			_empleadoService = empleadoService;
		}

		public async Task<IActionResult> Index()
		{
			var esSuperAdmin = User.IsInRole(EnvAuthService.RolSuperAdmin);
			var areas = User.FindAll("Area").Select(c => c.Value).ToList();
			var puedeOperadores = esSuperAdmin || areas.Contains("General") || areas.Contains("Operadores");

			var vm = new UsuariosIndexViewModel
			{
				PuedeGestionarAdministradores = esSuperAdmin,
				PuedeGestionarOperadores = puedeOperadores,
				Administradores = esSuperAdmin
					? (await _administradorService.ObtenerTodosAsync()).OrderBy(a => a.Usuario).ToList()
					: new List<Administrador>(),
				Operadores = puedeOperadores
					? (await _empleadoService.ObtenerTodosAsync()).OrderBy(e => e.Nombre).ToList()
					: new List<Empleado>()
			};

			return View(vm);
		}


		[HttpPost]
		[Authorize(Roles = "SuperAdmin")]
		public async Task<IActionResult> CrearAdministrador(string usuario, string contrasena, string? claveSupervisor, bool general, List<string>? areas)
		{
			var areasFinal = general ? new List<string> { "General" } : (areas ?? new List<string>());
			var (exito, error) = await _administradorService.CrearAsync(usuario, contrasena, areasFinal, claveSupervisor);
			TempData["ErrorAdmin"] = exito ? null : error;
			TempData["ExitoAdmin"] = exito ? "Administrador creado correctamente." : null;
			return RedirectToAction("Index");
		}

		[HttpPost]
		[Authorize(Roles = "SuperAdmin")]
		public async Task<IActionResult> EditarAdministrador(int id, string usuario, bool activo, string? nuevaContrasena, string? nuevaClaveSupervisor, bool general, List<string>? areas)
		{
			var areasFinal = general ? new List<string> { "General" } : (areas ?? new List<string>());
			var (exito, error) = await _administradorService.EditarAsync(id, usuario, activo, nuevaContrasena, areasFinal, nuevaClaveSupervisor);
			TempData["ErrorAdmin"] = exito ? null : error;
			return RedirectToAction("Index");
		}

		[HttpPost]
		[Authorize(Roles = "SuperAdmin")]
		public async Task<IActionResult> EliminarAdministrador(int id)
		{
			var exito = await _administradorService.EliminarAsync(id);
			TempData["ErrorAdmin"] = exito ? null : "No se pudo eliminar el administrador.";
			return RedirectToAction("Index");
		}


		[HttpPost]
		[RequiereArea("Operadores")]
		public async Task<IActionResult> CrearOperador(string nombre, string clave, RolEmpleado rol)
		{
			var (exito, error) = await _empleadoService.CrearAsync(nombre, clave, rol);
			TempData["ErrorOperador"] = exito ? null : error;
			TempData["ExitoOperador"] = exito ? "Operador creado correctamente." : null;
			return RedirectToAction("Index");
		}

		[HttpPost]
		[RequiereArea("Operadores")]
		public async Task<IActionResult> EditarOperador(int id, string nombre, bool activo, RolEmpleado rol, string? nuevaClave)
		{
			var (exito, error) = await _empleadoService.EditarAsync(id, nombre, activo, rol, nuevaClave);
			TempData["ErrorOperador"] = exito ? null : error;
			return RedirectToAction("Index");
		}

		[HttpPost]
		[RequiereArea("Operadores")]
		public async Task<IActionResult> EliminarOperador(int id)
		{
			var exito = await _empleadoService.EliminarAsync(id);
			TempData["ErrorOperador"] = exito ? null : "No se pudo eliminar el operador.";
			return RedirectToAction("Index");
		}
	}
}