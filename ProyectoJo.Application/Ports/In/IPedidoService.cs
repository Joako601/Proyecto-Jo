using ProyectoJo.Application.DTOs;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Application.Ports.In
{
	public interface IPedidoService
	{
		Task<List<Pedido>> ObtenerPendientesAsync();
		Task<Pedido?> ObtenerPorIdAsync(int id);
		Task<ResultadoCrearPedido> CrearAsync(Pedido pedido, string usuario, string estacion);
		Task<ResultadoCambiarEstado> CambiarEstadoAsync(int id, EstadoPedido nuevoEstado, string usuario, string estacion);
		Task<List<Pedido>> ObtenerParaCocinaAsync();
		Task<List<Pedido>> ObtenerParaRecepcionAsync();
	}
}