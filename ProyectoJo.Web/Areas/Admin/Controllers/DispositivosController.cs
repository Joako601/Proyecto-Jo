using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using ProyectoJo.Application.Ports.In;
using ProyectoJo.Web.Areas.Admin.Models;
using ProyectoJo.Web.Authorization;
using ProyectoJo.Web.Hubs;
using ProyectoJo.Web.Realtime;

namespace ProyectoJo.Web.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(AuthenticationSchemes = "JoCookieAuth")]
	[RequiereArea("Dispositivos")]
	public class DispositivosController : Controller
	{
		private readonly IDispositivoService _dispositivoService;
		private readonly DispositivoPresenceTracker _presencia;
		private readonly IHubContext<PedidosHub> _hub;

		public DispositivosController(IDispositivoService dispositivoService, DispositivoPresenceTracker presencia, IHubContext<PedidosHub> hub)
		{
			_dispositivoService = dispositivoService;
			_presencia = presencia;
			_hub = hub;
		}

		public async Task<IActionResult> Index()
		{
			var dispositivos = await _dispositivoService.ObtenerTodosAsync();
			var items = dispositivos
				.OrderByDescending(d => d.FechaRegistro)
				.Select(d => new DispositivoListItem { Dispositivo = d, Conectado = _presencia.EstaConectado(d.Token) })
				.ToList();

			return View(items);
		}

		[HttpPost]
		public async Task<IActionResult> ToggleBloqueado(int id)
		{
			var dispositivo = await _dispositivoService.ToggleBloqueadoAsync(id);
			if (dispositivo is not null && dispositivo.Bloqueado)
				await DesconectarDispositivoAsync(dispositivo.Token);

			return RedirectToAction("Index");
		}

		[HttpPost]
		public async Task<IActionResult> ToggleActivo(int id)
		{
			var dispositivo = await _dispositivoService.ToggleActivoAsync(id);
			if (dispositivo is not null && !dispositivo.Activo)
				await DesconectarDispositivoAsync(dispositivo.Token);

			return RedirectToAction("Index");
		}

		private async Task DesconectarDispositivoAsync(string token)
		{
			var conexiones = _presencia.ObtenerConexiones(token);
			if (conexiones.Count > 0)
				await _hub.Clients.Clients(conexiones).SendAsync("Desconectar");
		}
	}
}
