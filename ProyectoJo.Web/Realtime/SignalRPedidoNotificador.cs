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

		public async Task NotificarCreadoAsync(Pedido pedido)
		{
			await _hubContext.Clients.Group("Cocina").SendAsync("PedidoNuevo", pedido);
			await _hubContext.Clients.Group("Recepcion").SendAsync("PedidoNuevo", pedido);
		}


		public async Task NotificarEstadoCambiadoAsync(Pedido pedido)
		{
			await _hubContext.Clients.Group("Cocina").SendAsync("PedidoActualizado", pedido);
			await _hubContext.Clients.Group("Recepcion").SendAsync("PedidoActualizado", pedido);
		}
	}
}