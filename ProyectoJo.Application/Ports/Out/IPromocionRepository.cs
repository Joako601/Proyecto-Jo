using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Application.Ports.Out
{
	public interface IPromocionRepository
	{
		IEnumerable<Promocion> ObtenerTodas();
		Promocion? ObtenerPorId(int id);
		void Agregar(Promocion promocion);
		bool Editar(Promocion promocion);
		bool Eliminar(int id);
		bool ToggleActiva(int id);
	}
}