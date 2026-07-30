using Moq;
using Microsoft.Extensions.Logging;
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
		private readonly Mock<IAuditoriaService> _auditoriaService = new();
		private readonly PedidoUseCase _useCase;

		public PedidoUseCaseTests()
		{
			_useCase = new PedidoUseCase(
				_repository.Object,
				_finanzaService.Object,
				_notificador.Object,
				Mock.Of<IProductoService>(),
				Mock.Of<IPromocionService>(),
				Mock.Of<IInsumoService>(),
				Mock.Of<IRecetaService>(),
				_auditoriaService.Object,
				Mock.Of<ILogger<PedidoUseCase>>());
		}

		[Fact]
		public async Task CambiarEstadoAsync_AlPasarAPagado_RegistraMovimientoFinancieroUnaSolaVez()
		{
			// Arrange
			var pedidoAntes = new Pedido
			{
				Id = 1,
				Mesa = "5",
				Estado = EstadoPedido.Preparado,
				Items = new List<ItemPedido> { new() { ItemId = 1, Cantidad = 2, PrecioUnitario = 100m } }
			};
			var pedidoActualizado = new Pedido
			{
				Id = 1,
				Mesa = "5",
				Estado = EstadoPedido.Pagado,
				Items = pedidoAntes.Items // mismos items, mismo Total derivado (200m)
			};

			_repository
				.Setup(r => r.CambiarEstadoAtomicoAsync(1, EstadoPedido.Pagado, It.IsAny<Func<Pedido, Task<string?>>>()))
				.ReturnsAsync((pedidoAntes, pedidoActualizado, (string?)null));

			// Act
			var resultado = await _useCase.CambiarEstadoAsync(1, EstadoPedido.Pagado, "cajero1", "Recepción");

			// Assert
			Assert.True(resultado.Exitoso);
			Assert.Equal(EstadoPedido.Pagado, resultado.Pedido!.Estado);

			_finanzaService.Verify(f => f.RegistrarMovimiento(
				It.Is<Finanza>(fin => fin.Monto == 200m && fin.Categoria == "Ventas"),
				"Sistema (Pedido)"), Times.Once);

			_notificador.Verify(n => n.NotificarEstadoCambiadoAsync(pedidoActualizado), Times.Once);
		}

		[Fact]
		public async Task CambiarEstadoAsync_SiYaEstabaPagado_NoDuplicaElMovimientoFinanciero()
		{
			var pedidoYaPagado = new Pedido { Id = 1, Estado = EstadoPedido.Pagado };

			_repository
				.Setup(r => r.CambiarEstadoAtomicoAsync(1, EstadoPedido.Pagado, It.IsAny<Func<Pedido, Task<string?>>>()))
				.ReturnsAsync((pedidoYaPagado, pedidoYaPagado, (string?)null));

			await _useCase.CambiarEstadoAsync(1, EstadoPedido.Pagado, "cajero1", "Recepción");

			_finanzaService.Verify(f => f.RegistrarMovimiento(It.IsAny<Finanza>(), It.IsAny<string>()), Times.Never);
		}

		[Fact]
		public async Task CambiarEstadoAsync_SiElPedidoNoExiste_MarcaNoEncontradoSinTocarFinanzasNiNotificador()
		{
			_repository
				.Setup(r => r.CambiarEstadoAtomicoAsync(999, EstadoPedido.Pagado, It.IsAny<Func<Pedido, Task<string?>>>()))
				.ReturnsAsync(((Pedido?)null, (Pedido?)null, (string?)null));

			var resultado = await _useCase.CambiarEstadoAsync(999, EstadoPedido.Pagado, "cajero1", "Recepción");

			Assert.True(resultado.NoEncontrado);
			_finanzaService.Verify(f => f.RegistrarMovimiento(It.IsAny<Finanza>(), It.IsAny<string>()), Times.Never);
			_notificador.Verify(n => n.NotificarEstadoCambiadoAsync(It.IsAny<Pedido>()), Times.Never);
		}

		[Fact]
		public async Task CambiarEstadoAsync_CuandoElRepositorioRechazaLaTransicion_DevuelveMotivoRechazoSinNotificarNiAuditar()
		{
			var pedidoAntes = new Pedido { Id = 2, Estado = EstadoPedido.Pendiente };

			_repository
				.Setup(r => r.CambiarEstadoAtomicoAsync(2, EstadoPedido.Preparado, It.IsAny<Func<Pedido, Task<string?>>>()))
				.ReturnsAsync((pedidoAntes, (Pedido?)null, "Sin stock de ingredientes"));

			var resultado = await _useCase.CambiarEstadoAsync(2, EstadoPedido.Preparado, "cocina1", "Cocina");

			Assert.False(resultado.Exitoso);
			Assert.Equal("Sin stock de ingredientes", resultado.MotivoRechazo);
			_notificador.Verify(n => n.NotificarEstadoCambiadoAsync(It.IsAny<Pedido>()), Times.Never);
			_auditoriaService.Verify(a => a.RegistrarAccion(
				It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TipoAccionAuditoria>(), It.IsAny<string>(),
				It.IsAny<string?>(), It.IsAny<string?>()), Times.Never);
		}

		[Fact]
		public async Task CambiarEstadoAsync_AlActualizarConExito_RegistraAuditoriaConElUsuarioYLaEstacion()
		{
			var pedidoAntes = new Pedido { Id = 3, Estado = EstadoPedido.Pendiente };
			var pedidoActualizado = new Pedido { Id = 3, Estado = EstadoPedido.Preparado };

			_repository
				.Setup(r => r.CambiarEstadoAtomicoAsync(3, EstadoPedido.Preparado, It.IsAny<Func<Pedido, Task<string?>>>()))
				.ReturnsAsync((pedidoAntes, pedidoActualizado, (string?)null));

			await _useCase.CambiarEstadoAsync(3, EstadoPedido.Preparado, "cocina1", "Cocina");

			_auditoriaService.Verify(a => a.RegistrarAccion(
				"cocina1", "Cocina", TipoAccionAuditoria.Edicion, "Pedido #3",
				"Pendiente", "Preparado"), Times.Once);
		}
	}
}
