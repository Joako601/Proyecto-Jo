using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using ProyectoJo.Web.Realtime;

namespace ProyectoJo.Web.Hubs
{
	[Authorize(AuthenticationSchemes = "OperacionesCookieAuth")]
	public class PedidosHub : Hub
	{
		private readonly DispositivoPresenceTracker _presencia;

		public PedidosHub(DispositivoPresenceTracker presencia)
		{
			_presencia = presencia;
		}

		public async Task UnirseAGrupo(string grupo)
		{
			var rolesPermitidos = new[] { "Cocina", "Recepcion" };
			if (!rolesPermitidos.Contains(grupo)) return;

			if (!Context.User!.IsInRole(grupo)) return;

			await Groups.AddToGroupAsync(Context.ConnectionId, grupo);
		}

		public override Task OnConnectedAsync()
		{
			var token = Context.User?.FindFirst("DispositivoToken")?.Value;
			if (!string.IsNullOrEmpty(token))
				_presencia.Conectar(token, Context.ConnectionId);

			return base.OnConnectedAsync();
		}

		public override Task OnDisconnectedAsync(Exception? exception)
		{
			var token = Context.User?.FindFirst("DispositivoToken")?.Value;
			if (!string.IsNullOrEmpty(token))
				_presencia.Desconectar(token, Context.ConnectionId);

			return base.OnDisconnectedAsync(exception);
		}
	}
}