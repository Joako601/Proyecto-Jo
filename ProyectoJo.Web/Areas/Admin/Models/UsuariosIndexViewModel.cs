using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Web.Areas.Admin.Models
{
	public class UsuariosIndexViewModel
	{
		public bool PuedeGestionarAdministradores { get; set; }
		public bool PuedeGestionarOperadores { get; set; }
		public List<Administrador> Administradores { get; set; } = new();
		public List<Empleado> Operadores { get; set; } = new();
	}
}