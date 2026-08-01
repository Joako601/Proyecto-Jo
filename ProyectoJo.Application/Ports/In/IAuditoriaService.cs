using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Application.Ports.In
{
	public interface IAuditoriaService
	{
		void RegistrarAccion(string usuario, string modulo, TipoAccionAuditoria accion, string entidad,
			string? detalleAntes = null, string? detalleDespues = null);

		List<RegistroAuditoria> ObtenerHistorial(string? modulo = null, DateTime? desde = null, DateTime? hasta = null);

		(List<RegistroAuditoria> Items, int Total) ObtenerHistorialPaginado(
			string? modulo, DateTime? desde, DateTime? hasta, int pagina, int porPagina);
	}
}
