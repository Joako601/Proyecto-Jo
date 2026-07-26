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
		private readonly ILogger<CocinaController> _logger;

		public CocinaController(IPedidoService pedidoService, ILogger<CocinaController> logger)
		{
			_pedidoService = pedidoService;
			_logger = logger;
		}

		// GET /Operaciones/Cocina
		public IActionResult Index() => View();

		// GET /Operaciones/Cocina/ObtenerPedidos
		
		[HttpGet]
		public async Task<IActionResult> ObtenerPedidos()
		{
			try
			{
				var pedidos = await _pedidoService.ObtenerParaCocinaAsync();
				return Json(pedidos);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al obtener pedidos para Cocina");
				return StatusCode(500, new { error = "No se pudieron cargar los pedidos. Intentá de nuevo." });
			}
		}

		// POST /Operaciones/Cocina/CambiarEstado
		[HttpPost]
		public async Task<IActionResult> CambiarEstado(int id, string nuevoEstado)
		{
			if (!Enum.TryParse<EstadoPedido>(nuevoEstado, ignoreCase: true, out var estado))
				return BadRequest(new { error = $"Estado inválido: '{nuevoEstado}'" });
			try
			{
				var resultado = await _pedidoService.CambiarEstadoAsync(id, estado);

				if (resultado.NoEncontrado) return NotFound(new { error = $"Pedido #{id} no encontrado." });
				if (!resultado.Exitoso) return Conflict(new { error = resultado.MotivoRechazo });

				return Json(resultado.Pedido);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al cambiar estado del pedido #{PedidoId}", id);
				return StatusCode(500, new { error = "No se pudo cambiar el estado. Intentá de nuevo." });
			}
		}
	}
}