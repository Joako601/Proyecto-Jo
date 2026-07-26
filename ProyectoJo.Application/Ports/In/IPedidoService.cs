using ProyectoJo.Application.DTOs;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Application.Ports.In
{
	public interface IPedidoService
	{
		Task<List<Pedido>> ObtenerPendientesAsync();
		Task<Pedido?> ObtenerPorIdAsync(int id);
		Task<ResultadoCrearPedido> CrearAsync(Pedido pedido);
		Task<ResultadoCambiarEstado> CambiarEstadoAsync(int id, EstadoPedido nuevoEstado);
		Task<List<Pedido>> ObtenerParaCocinaAsync();
		Task<List<Pedido>> ObtenerParaRecepcionAsync();
		Task<ResumenMapaCalor> ObtenerMapaCalorAsync(
			DateTime? desde = null,
			DateTime? hasta = null,
			bool semanaHistoricoCompleto = true,
			int semanaOffset = 0,
			int? anioMeses = null,
			int? mesDetalle = null);
	}
}