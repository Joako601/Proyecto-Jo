using Moq;
using ProyectoJo.Application.Ports.In;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Application.UseCases;
using ProyectoJo.Domain.Entities;
using Xunit;

namespace ProyectoJo.Application.Tests.UseCases
{
	public class PedidoUseCaseTests
	{
		private readonly Mock<IPedidoRepository> _repository = new();
		private readonly Mock<IFinanzaService> _finanzaService = new();
		private readonly Mock<IPedidoNotificador> _notificador = new();
		private readonly Mock<IProductoService> _productoService = new();
		private readonly Mock<IPromocionService> _promocionService = new();
		private readonly Mock<Microsoft.Extensions.Logging.ILogger<PedidoUseCase>> _logger = new();
		private readonly PedidoUseCase _useCase;

		public PedidoUseCaseTests()
		{
			_useCase = new PedidoUseCase(
				_repository.Object,
				_finanzaService.Object,
				_notificador.Object,
				_productoService.Object,
				_promocionService.Object,
				_logger.Object);
		}

		[Fact]
		public async Task CambiarEstadoAsync_CuandoElPedidoNoExiste_DevuelveNullYNoRegistraFinanzaNiNotifica()
		{
			// Arrange
			_repository.Setup(r => r.ObtenerPorIdAsync(999)).ReturnsAsync((Pedido?)null);

			// Act
			var resultado = await _useCase.CambiarEstadoAsync(999, EstadoPedido.Preparado);

			// Assert
			Assert.Null(resultado);
			_repository.Verify(r => r.CambiarEstadoAtomicoAsync(It.IsAny<int>(), It.IsAny<EstadoPedido>()), Times.Never);
			_finanzaService.Verify(f => f.RegistrarMovimiento(It.IsAny<Finanza>(), It.IsAny<string>()), Times.Never);
			_notificador.Verify(n => n.NotificarEstadoCambiadoAsync(It.IsAny<Pedido>()), Times.Never);
		}

		[Fact]
		public async Task CambiarEstadoAsync_UsaElCambioAtomicoEnVezDeLeerMutarYActualizar()
		{
			// Arrange: el pedido existe y está Pendiente
			var pedidoExistente = new Pedido { Id = 1, Mesa = "5", Estado = EstadoPedido.Pendiente };
			_repository.Setup(r => r.ObtenerPorIdAsync(1)).ReturnsAsync(pedidoExistente);

			var pedidoActualizado = new Pedido { Id = 1, Mesa = "5", Estado = EstadoPedido.Preparado };
			_repository.Setup(r => r.CambiarEstadoAtomicoAsync(1, EstadoPedido.Preparado)).ReturnsAsync(pedidoActualizado);

			// Act
			var resultado = await _useCase.CambiarEstadoAsync(1, EstadoPedido.Preparado);

			// Assert: se llamó al método atómico (no a ActualizarAsync con el objeto en memoria)
			Assert.Equal(EstadoPedido.Preparado, resultado!.Estado);
			_repository.Verify(r => r.CambiarEstadoAtomicoAsync(1, EstadoPedido.Preparado), Times.Once);
			_repository.Verify(r => r.ActualizarAsync(It.IsAny<Pedido>()), Times.Never);
		}

		[Fact]
		public async Task CambiarEstadoAsync_CuandoPasaAPagadoPorPrimeraVez_RegistraMovimientoFinanciero()
		{
			// Arrange: pedido existente, no estaba pagado
			var pedidoExistente = new Pedido
			{
				Id = 1,
				Mesa = "5",
				Estado = EstadoPedido.Preparado,
				Items = new List<ItemPedido> { new() { ItemId = 1, Nombre = "Tacos", Cantidad = 2, PrecioUnitario = 50 } }
			};
			_repository.Setup(r => r.ObtenerPorIdAsync(1)).ReturnsAsync(pedidoExistente);

			var pedidoPagado = new Pedido
			{
				Id = 1,
				Mesa = "5",
				Estado = EstadoPedido.Pagado,
				Items = pedidoExistente.Items
			};
			_repository.Setup(r => r.CambiarEstadoAtomicoAsync(1, EstadoPedido.Pagado)).ReturnsAsync(pedidoPagado);

			// Act
			await _useCase.CambiarEstadoAsync(1, EstadoPedido.Pagado);

			// Assert
			_finanzaService.Verify(f => f.RegistrarMovimiento(
				It.Is<Finanza>(fin => fin.Monto == 100 && fin.Tipo == TipoMovimiento.Ingreso),
				"Sistema (Pedido)"), Times.Once);
		}

		[Fact]
		public async Task CambiarEstadoAsync_CuandoYaEstabaPagado_NoVuelveARegistrarMovimientoFinanciero()
		{
			// Arrange: el pedido YA estaba pagado antes de esta llamada
			var pedidoExistente = new Pedido { Id = 1, Mesa = "5", Estado = EstadoPedido.Pagado };
			_repository.Setup(r => r.ObtenerPorIdAsync(1)).ReturnsAsync(pedidoExistente);

			var pedidoActualizado = new Pedido { Id = 1, Mesa = "5", Estado = EstadoPedido.Pagado };
			_repository.Setup(r => r.CambiarEstadoAtomicoAsync(1, EstadoPedido.Pagado)).ReturnsAsync(pedidoActualizado);

			// Act
			await _useCase.CambiarEstadoAsync(1, EstadoPedido.Pagado);

			// Assert: no se duplica el movimiento financiero
			_finanzaService.Verify(f => f.RegistrarMovimiento(It.IsAny<Finanza>(), It.IsAny<string>()), Times.Never);
		}
	}
}