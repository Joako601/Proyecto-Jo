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
			try
			{
				var menu = _productoService.ObtenerMenu();
				return Json(menu);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al obtener el menú para Recepción");
				return StatusCode(500, new { error = "No se pudo cargar el menú. Intentá de nuevo." });
			}
		}

		// GET /Operaciones/Recepcion/ObtenerPedidos
		[HttpGet]
		public async Task<IActionResult> ObtenerPedidos()
		{
			try
			{
				var pedidos = await _pedidoService.ObtenerParaRecepcionAsync();
				return Json(pedidos);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al obtener pedidos para Recepción");
				return StatusCode(500, new { error = "No se pudieron cargar los pedidos. Intentá de nuevo." });
			}
		}

		// POST /Operaciones/Recepcion/Crear
		[HttpPost]
		public async Task<IActionResult> Crear([FromBody] Pedido pedido)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);
			try
			{
				var resultado = await _pedidoService.CrearAsync(pedido);
				return Json(resultado);
			}
			catch (InvalidOperationException ex)
			{
				return BadRequest(new { error = ex.Message });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error inesperado al crear pedido");
				return StatusCode(500, new { error = "Ocurrió un error al crear el pedido. Intenta de nuevo." });
			}
		}

		// POST /Operaciones/Recepcion/Pagar
		[HttpPost]
		public async Task<IActionResult> Pagar(int id)
		{
			try
			{
				var resultado = await _pedidoService.CambiarEstadoAsync(id, EstadoPedido.Pagado);

				if (resultado.NoEncontrado) return NotFound($"Pedido #{id} no encontrado.");
				if (!resultado.Exitoso) return Conflict(new { error = resultado.MotivoRechazo });

				return Json(resultado.Pedido);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al pagar el pedido #{PedidoId}", id);
				return StatusCode(500, "Ocurrió un error al procesar el pago. Intenta de nuevo.");
			}
		}
	}
}