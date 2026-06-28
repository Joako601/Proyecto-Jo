using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoJo.Application.Ports.In;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Web.Areas.Operaciones.Controllers
{
	[Area("Operaciones")]
	[Authorize(AuthenticationSchemes = "OperacionesCookieAuth", Roles = "Recepcion")]
	public class RecepcionController : Controller
	{
		private readonly IPedidoService _pedidoService;
		private readonly IProductoService _productoService;
		private readonly ILogger<RecepcionController> _logger;

		public RecepcionController(IPedidoService pedidoService, IProductoService productoService, ILogger<RecepcionController> logger)
		{
			_pedidoService = pedidoService;
			_productoService = productoService;
			_logger = logger;
		}

		// GET /Operaciones/Recepcion
		public IActionResult Index() => View();

		// GET /Operaciones/Recepcion/ObtenerMenu 
		[HttpGet]
		public IActionResult ObtenerMenu()
		{
			var menu = _productoService.ObtenerMenu();
			return Json(menu);
		}

		// GET /Operaciones/Recepcion/ObtenerPedidos
		[HttpGet]
		public async Task<IActionResult> ObtenerPedidos()
		{
			var pedidos = await _pedidoService.ObtenerParaRecepcionAsync();
			return Json(pedidos);
		}

		// POST /Operaciones/Recepcion/Crear
		[HttpPost]
		public async Task<IActionResult> Crear([FromBody] Pedido pedido)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);
			var creado = await _pedidoService.CrearAsync(pedido);
			return Json(creado);
		}

		// POST /Operaciones/Recepcion/Pagar
		[HttpPost]
		public async Task<IActionResult> Pagar(int id)
		{
			try
			{
				var actualizado = await _pedidoService.CambiarEstadoAsync(id, EstadoPedido.Pagado);
				if (actualizado is null) return NotFound($"Pedido #{id} no encontrado.");
				return Json(actualizado);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al pagar el pedido #{PedidoId}", id);
				return StatusCode(500, "Ocurrió un error al procesar el pago. Intenta de nuevo.");
			}
		}
	}
}