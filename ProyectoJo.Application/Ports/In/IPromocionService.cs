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

		void Agregar(Promocion promocion, string usuario);
		bool Editar(Promocion promocion, string usuario);
		bool Eliminar(int id, string usuario);
		void ToggleActiva(int id, string usuario);

		bool EstaVigente(Promocion promocion);
		decimal CalcularPrecioFinal(Item item);
	}
}