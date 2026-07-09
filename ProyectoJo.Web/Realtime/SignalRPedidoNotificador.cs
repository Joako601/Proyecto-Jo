using Microsoft.AspNetCore.SignalR;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Domain.Entities;
using ProyectoJo.Web.Hubs;

namespace ProyectoJo.Web.Realtime
{
	public class SignalRPedidoNotificador : IPedidoNotificador
	{
		private readonly IHubContext<PedidosHub> _hubContext;

		public SignalRPedidoNotificador(IHubContext<PedidosHub> hubContext)
		{
			_hubContext = hubContext;
		}

		public Task NotificarCreadoAsync(Pedido pedido) =>
			Task.WhenAll(
				_hubContext.Clients.Group("Cocina").SendAsync("PedidoNuevo", pedido),
				_hubContext.Clients.Group("Recepcion").SendAsync("PedidoNuevo", pedido)
			);

		public Task NotificarEstadoCambiadoAsync(Pedido pedido) =>
			Task.WhenAll(
				_hubContext.Clients.Group("Cocina").SendAsync("PedidoActualizado", pedido),
				_hubContext.Clients.Group("Recepcion").SendAsync("PedidoActualizado", pedido)
			);
	}
}