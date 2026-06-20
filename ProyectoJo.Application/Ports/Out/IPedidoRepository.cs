using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Application.Ports.Out
{
	public interface IPedidoRepository
	{
		Task<List<Pedido>> ObtenerTodosAsync();
		Task<Pedido?> ObtenerPorIdAsync(int id);
		Task<Pedido> GuardarAsync(Pedido pedido);
		Task<Pedido?> ActualizarAsync(Pedido pedido);
	}
}