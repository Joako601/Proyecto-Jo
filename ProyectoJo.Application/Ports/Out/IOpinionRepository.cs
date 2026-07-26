using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Application.Ports.Out
{
	public interface IOpinionRepository
	{
		List<OpinionCliente> ObtenerTodas();
		OpinionCliente? ObtenerPorId(int id);
		void Agregar(OpinionCliente opinion);
		bool Editar(OpinionCliente opinion);
		bool Eliminar(int id);
	}
}