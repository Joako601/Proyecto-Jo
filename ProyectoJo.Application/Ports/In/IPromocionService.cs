using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Application.Ports.In
{
	public interface IPromocionService
	{
		IEnumerable<Promocion> ObtenerTodas();
		IEnumerable<Promocion> ObtenerVigentes();
		IEnumerable<Promocion> ObtenerVigentesGenerales();
		Promocion? ObtenerPorId(int id);

		void Agregar(Promocion promocion, string usuario);
		bool Editar(Promocion promocion, string usuario);
		bool Eliminar(int id, string usuario);
		bool ToggleActiva(int id, string usuario);

		bool ActualizarFecha(int id, DateTime? fechaInicio, DateTime? fechaFin, string usuario);
		bool HacerPermanente(int id, string usuario);

		bool EstaVigente(Promocion promocion);
		decimal CalcularPrecioFinal(Item item);
		decimal CalcularPrecioFinal(Item item, List<Promocion> promosVigentes);
	}
}