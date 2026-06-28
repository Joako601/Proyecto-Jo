using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Application.Ports.Out
{
	public interface IAuditoriaRepository
	{
		List<RegistroAuditoria> ObtenerTodos();
		void Guardar(RegistroAuditoria registro);
	}
}
