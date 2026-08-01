using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Application.Ports.Out
{
	public interface IAuditoriaRepository
	{
		List<RegistroAuditoria> ObtenerTodos();
		(List<RegistroAuditoria> Items, int Total) ObtenerPaginado(
			string? modulo, DateTime? desde, DateTime? hasta, int pagina, int porPagina);
		void Guardar(RegistroAuditoria registro);
	}
}
