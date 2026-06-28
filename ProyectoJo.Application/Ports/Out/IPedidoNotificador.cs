using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Application.Ports.Out
{
	public interface IPedidoNotificador
	{
		Task NotificarCreadoAsync(Pedido pedido);
		Task NotificarEstadoCambiadoAsync(Pedido pedido);
	}
}