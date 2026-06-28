using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Application.Ports.In
{
	public interface IAuditoriaService
	{
		void RegistrarAccion(string usuario, string modulo, TipoAccionAuditoria accion, string entidad,
			string? detalleAntes = null, string? detalleDespues = null);

		List<RegistroAuditoria> ObtenerHistorial(string? modulo = null, DateTime? desde = null, DateTime? hasta = null);
	}
}
