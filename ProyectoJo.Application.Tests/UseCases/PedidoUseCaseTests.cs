using Moq;
using Microsoft.Extensions.Logging;
using ProyectoJo.Application.Ports.In;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Application.UseCases;
using ProyectoJo.Domain.Entities;
using Xunit;

public class PedidoUseCase_CambiarEstadoAsync_Tests
{
	[Fact]
	public async Task Al_pasar_a_Pagado_registra_movimiento_financiero_una_sola_vez()
	{
		// Arrange
		var mockRepo = new Mock<IPedidoRepository>();
		var mockFinanzas = new Mock<IFinanzaService>();
		var mockNotificador = new Mock<IPedidoNotificador>();
		var mockProductos = new Mock<IProductoService>();
		var mockPromos = new Mock<IPromocionService>();
		var mockLogger = new Mock<ILogger<PedidoUseCase>>();

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

		mockRepo
			.Setup(r => r.CambiarEstadoAtomicoAsync(1, EstadoPedido.Pagado))
			.ReturnsAsync((pedidoAntes, pedidoActualizado)); // 👈 la parte que te tiraba el error

		var useCase = new PedidoUseCase(
			mockRepo.Object,
			mockFinanzas.Object,
			mockNotificador.Object,
			mockProductos.Object,
			mockPromos.Object,
			mockLogger.Object);

		// Act
		var resultado = await useCase.CambiarEstadoAsync(1, EstadoPedido.Pagado);

		// Assert
		Assert.NotNull(resultado);
		Assert.Equal(EstadoPedido.Pagado, resultado!.Estado);

		mockFinanzas.Verify(f => f.RegistrarMovimiento(
			It.Is<Finanza>(fin => fin.Monto == 200m && fin.Categoria == "Ventas"),
			"Sistema (Pedido)"), Times.Once);

		mockNotificador.Verify(n => n.NotificarEstadoCambiadoAsync(pedidoActualizado), Times.Once);
	}

	[Fact]
	public async Task Si_ya_estaba_Pagado_no_duplica_el_movimiento_financiero()
	{
		var mockRepo = new Mock<IPedidoRepository>();
		var mockFinanzas = new Mock<IFinanzaService>();
		var mockNotificador = new Mock<IPedidoNotificador>();
		var mockProductos = new Mock<IProductoService>();
		var mockPromos = new Mock<IPromocionService>();
		var mockLogger = new Mock<ILogger<PedidoUseCase>>();

		var pedidoYaPagado = new Pedido { Id = 1, Estado = EstadoPedido.Pagado };

		mockRepo
			.Setup(r => r.CambiarEstadoAtomicoAsync(1, EstadoPedido.Pagado))
			.ReturnsAsync((pedidoYaPagado, pedidoYaPagado)); 

		var useCase = new PedidoUseCase(
			mockRepo.Object, mockFinanzas.Object, mockNotificador.Object,
			mockProductos.Object, mockPromos.Object, mockLogger.Object);

		await useCase.CambiarEstadoAsync(1, EstadoPedido.Pagado);

		mockFinanzas.Verify(f => f.RegistrarMovimiento(It.IsAny<Finanza>(), It.IsAny<string>()), Times.Never);
	}

	[Fact]
	public async Task Si_el_pedido_no_existe_devuelve_null_sin_tocar_finanzas_ni_notificador()
	{
		var mockRepo = new Mock<IPedidoRepository>();
		mockRepo
			.Setup(r => r.CambiarEstadoAtomicoAsync(999, EstadoPedido.Pagado))
			.ReturnsAsync(((Pedido?)null, (Pedido?)null)); 

		var mockFinanzas = new Mock<IFinanzaService>();
		var mockNotificador = new Mock<IPedidoNotificador>();

		var useCase = new PedidoUseCase(
			mockRepo.Object, mockFinanzas.Object, mockNotificador.Object,
			Mock.Of<IProductoService>(), Mock.Of<IPromocionService>(),
			Mock.Of<ILogger<PedidoUseCase>>());

		var resultado = await useCase.CambiarEstadoAsync(999, EstadoPedido.Pagado);

		Assert.Null(resultado);
		mockFinanzas.Verify(f => f.RegistrarMovimiento(It.IsAny<Finanza>(), It.IsAny<string>()), Times.Never);
		mockNotificador.Verify(n => n.NotificarEstadoCambiadoAsync(It.IsAny<Pedido>()), Times.Never);
	}
}