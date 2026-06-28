using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ProyectoJo.Web.Hubs
{
	[Authorize(AuthenticationSchemes = "OperacionesCookieAuth")]
	public class PedidosHub : Hub
	{

		public async Task UnirseAGrupo(string grupo)
		{
			var rolesPermitidos = new[] { "Cocina", "Recepcion" };
			if (!rolesPermitidos.Contains(grupo)) return;

			if (!Context.User!.IsInRole(grupo)) return;

			await Groups.AddToGroupAsync(Context.ConnectionId, grupo);
		}
	}
}