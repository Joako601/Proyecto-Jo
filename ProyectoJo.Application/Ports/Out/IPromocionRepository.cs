using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Application.Ports.Out
{
	public interface IPromocionRepository
	{
		IEnumerable<Promocion> ObtenerTodas();
		Promocion? ObtenerPorId(int id);
		void Agregar(Promocion promocion);
		void Editar(Promocion promocion);
		void Eliminar(int id);
		void ToggleActiva(int id);
	}
}