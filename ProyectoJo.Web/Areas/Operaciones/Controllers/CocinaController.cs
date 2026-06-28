using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoJo.Application.Ports.In;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Web.Areas.Operaciones.Controllers
{
	[Area("Operaciones")]
	[Authorize(AuthenticationSchemes = "OperacionesCookieAuth", Roles = "Cocina")]
	public class CocinaController : Controller
	{
		private readonly IPedidoService _pedidoService;

		public CocinaController(IPedidoService pedidoService)
		{
			_pedidoService = pedidoService;
		}

		// GET /Operaciones/Cocina
		public IActionResult Index() => View();

		// GET /Operaciones/Cocina/ObtenerPedidos
		[HttpGet]
		public async Task<IActionResult> ObtenerPedidos()
		{
			var pedidos = await _pedidoService.ObtenerParaCocinaAsync();
			return Json(pedidos);
		}

		// POST /Operaciones/Cocina/CambiarEstado
		[HttpPost]
		public async Task<IActionResult> CambiarEstado(int id, string nuevoEstado)
		{
			if (!Enum.TryParse<EstadoPedido>(nuevoEstado, ignoreCase: true, out var estado))
				return BadRequest($"Estado inválido: '{nuevoEstado}'");

			var actualizado = await _pedidoService.CambiarEstadoAsync(id, estado);
			if (actualizado is null) return NotFound($"Pedido #{id} no encontrado.");
			return Json(actualizado);
		}
	}
}