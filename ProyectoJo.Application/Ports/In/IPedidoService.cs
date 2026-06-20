using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Application.Ports.In
{
	public interface IPedidoService
	{
		Task<List<Pedido>> ObtenerPendientesAsync();
		Task<Pedido?> ObtenerPorIdAsync(int id);
		Task<Pedido> CrearAsync(Pedido pedido);
		Task<Pedido?> CambiarEstadoAsync(int id, EstadoPedido nuevoEstado);
		Task<List<Pedido>> ObtenerParaCocinaAsync();       // ← NUEVO
		Task<List<Pedido>> ObtenerParaRecepcionAsync();
	}
}