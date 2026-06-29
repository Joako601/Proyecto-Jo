using ProyectoJo.Domain.Entities;

public interface IPedidoRepository
{
	Task<List<Pedido>> ObtenerTodosAsync();
	Task<Pedido?> ObtenerPorIdAsync(int id);
	Task<Pedido> GuardarAsync(Pedido pedido);
	Task<Pedido?> ActualizarAsync(Pedido pedido);
	Task<Pedido?> CambiarEstadoAtomicoAsync(int id, EstadoPedido nuevoEstado);
}