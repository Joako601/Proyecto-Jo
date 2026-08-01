using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Web.Areas.Admin.Models
{
	public class DispositivoListItem
	{
		public DispositivoOperaciones Dispositivo { get; set; } = null!;
		public bool Conectado { get; set; }
	}
}
