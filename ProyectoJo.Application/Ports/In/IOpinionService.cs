using ProyectoJo.Application.DTOs;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Application.Ports.In
{
	public interface IOpinionService
	{
		List<OpinionDto> ObtenerTodas();
		OpinionCliente? ObtenerPorId(int id);
		void Agregar(OpinionCliente opinion, string usuario);
		bool Editar(OpinionCliente opinion, string usuario);
		bool Eliminar(int id, string usuario);
		int ContarTotal();
		int ContarPorEstado(EstadoSemaforo estado);
	}
}