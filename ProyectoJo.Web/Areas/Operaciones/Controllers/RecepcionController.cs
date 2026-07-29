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
		private readonly IInsumoService _insumoService;
		private readonly ILogger<RecepcionController> _logger;

		public RecepcionController(
			IPedidoService pedidoService,
			IProductoService productoService,
			IInsumoService insumoService,
			ILogger<RecepcionController> logger)
		{
			_pedidoService = pedidoService;
			_productoService = productoService;
			_insumoService = insumoService;
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

				var menuConStock = menu.Select(item =>
				{
					var stockMaximo = _insumoService.ObtenerMaximoDisponible(item);
					return new
					{
						item.Id,
						item.Platillo,
						item.Categoria,
						item.Precio,
						item.Descripcion,
						item.Base,
						item.Activo,
						item.ImagenUrl,
						item.Ingredientes,
						Agotado = item.Agotado || stockMaximo == 0,
						StockMaximo = stockMaximo
					};
				});

				return Json(menuConStock);
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
				var resultado = await _pedidoService.CrearAsync(pedido, User.Identity?.Name ?? "Desconocido", "Recepcion");
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
				var resultado = await _pedidoService.CambiarEstadoAsync(id, EstadoPedido.Pagado, User.Identity?.Name ?? "Desconocido", "Recepcion");

				if (resultado.NoEncontrado) return NotFound($"Pedido #{id} no encontrado.");
				if (!resultado.Exitoso) return Conflict(new { error = resultado.MotivoRechazo });

				if (resultado.AdvertenciaRegistroFinanciero is not null)
					_logger.LogWarning("Pedido #{PedidoId} pagado con advertencia: {Advertencia}", id, resultado.AdvertenciaRegistroFinanciero);

				return Json(new { pedido = resultado.Pedido, advertencia = resultado.AdvertenciaRegistroFinanciero });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al pagar el pedido #{PedidoId}", id);
				return StatusCode(500, "Ocurrió un error al procesar el pago. Intenta de nuevo.");
			}
		}
	}
}