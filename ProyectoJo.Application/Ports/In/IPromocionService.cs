using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Application.Ports.In
{
	public interface IPromocionService
	{
		IEnumerable<Promocion> ObtenerTodas();
		IEnumerable<Promocion> ObtenerVigentes();
		IEnumerable<Promocion> ObtenerVigentesGenerales();
		IEnumerable<Promocion> ObtenerVigentesPorItem(int itemId);
		Promocion? ObtenerPorId(int id);

		void Agregar(Promocion promocion);
		void Editar(Promocion promocion);
		void Eliminar(int id);
		void ToggleActiva(int id);

		bool EstaVigente(Promocion promocion);
		decimal CalcularPrecioFinal(Item item);
	}
}