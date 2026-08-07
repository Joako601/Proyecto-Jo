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
		private readonly Mock<IProductoService> _productoService = new();
		private readonly Mock<IPromocionService> _promocionService = new();
		private readonly Mock<IInsumoService> _insumoService = new();
		private readonly Mock<IRecetaService> _recetaService = new();
		private readonly Mock<IAuditoriaService> _auditoriaService = new();
		private readonly PedidoUseCase _useCase;

		public PedidoUseCaseTests()
		{
			_promocionService.Setup(p => p.ObtenerVigentes()).Returns(new List<Promocion>());
			_insumoService.Setup(i => i.ObtenerIndicePorNombre()).Returns(new Dictionary<string, Insumo>());
			_repository.Setup(r => r.GuardarAsync(It.IsAny<Pedido>())).Returns((Pedido p) => Task.FromResult(p));

			_useCase = new PedidoUseCase(
				_repository.Object,
				_finanzaService.Object,
				_notificador.Object,
				_productoService.Object,
				_promocionService.Object,
				_insumoService.Object,
				_recetaService.Object,
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

		[Fact]
		public async Task CrearAsync_ConCantidadInvalida_DescartaLaLineaPeroConservaLasDemas()
		{
			// Arrange
			_productoService.Setup(p => p.ObtenerTodos()).Returns(new List<Item>
			{
				new() { Id = 1, Platillo = "Agua", Precio = 20m, Activo = true },
				new() { Id = 2, Platillo = "Taco", Precio = 50m, Activo = true }
			});
			var pedido = new Pedido
			{
				Mesa = "5",
				Items = new List<ItemPedido>
				{
					new() { ItemId = 1, Nombre = "Agua", Cantidad = 0 },
					new() { ItemId = 2, Nombre = "Taco", Cantidad = 1 }
				}
			};

			// Act
			var resultado = await _useCase.CrearAsync(pedido, "mesero1", "Recepción");

			// Assert
			Assert.Single(resultado.LineasDescartadas);
			Assert.Equal("Cantidad inválida", resultado.LineasDescartadas[0].Motivo);
			Assert.Single(resultado.Pedido.Items);
			Assert.Equal(2, resultado.Pedido.Items[0].ItemId);
		}

		[Fact]
		public async Task CrearAsync_ConItemInexistenteOInactivo_DescartaLaLineaConMotivoDeMenu()
		{
			// Arrange
			_productoService.Setup(p => p.ObtenerTodos()).Returns(new List<Item>
			{
				new() { Id = 2, Platillo = "Taco", Precio = 50m, Activo = false },
				new() { Id = 3, Platillo = "Refresco", Precio = 30m, Activo = true }
			});
			var pedido = new Pedido
			{
				Mesa = "5",
				Items = new List<ItemPedido>
				{
					new() { ItemId = 1, Nombre = "Fantasma", Cantidad = 1 },
					new() { ItemId = 2, Nombre = "Taco", Cantidad = 1 },
					new() { ItemId = 3, Nombre = "Refresco", Cantidad = 1 }
				}
			};

			// Act
			var resultado = await _useCase.CrearAsync(pedido, "mesero1", "Recepción");

			// Assert
			Assert.Equal(2, resultado.LineasDescartadas.Count);
			Assert.All(resultado.LineasDescartadas, l => Assert.Equal("Ya no está disponible en el menú", l.Motivo));
			Assert.Single(resultado.Pedido.Items);
		}

		[Fact]
		public async Task CrearAsync_ConItemAgotado_DescartaLaLineaConMotivoDeStock()
		{
			// Arrange
			_productoService.Setup(p => p.ObtenerTodos()).Returns(new List<Item>
			{
				new() { Id = 1, Platillo = "Taco", Precio = 50m, Activo = true, Agotado = true },
				new() { Id = 2, Platillo = "Refresco", Precio = 30m, Activo = true }
			});
			var pedido = new Pedido
			{
				Mesa = "5",
				Items = new List<ItemPedido>
				{
					new() { ItemId = 1, Nombre = "Taco", Cantidad = 1 },
					new() { ItemId = 2, Nombre = "Refresco", Cantidad = 1 }
				}
			};

			// Act
			var resultado = await _useCase.CrearAsync(pedido, "mesero1", "Recepción");

			// Assert
			Assert.Single(resultado.LineasDescartadas);
			Assert.Equal("Sin stock en este momento", resultado.LineasDescartadas[0].Motivo);
		}

		[Fact]
		public async Task CrearAsync_ConStockDeInsumosMenorALoSolicitado_AjustaLaCantidadYLaRegistraComoAjustada()
		{
			// Arrange
			var item = new Item { Id = 1, Platillo = "Taco", Precio = 50m, Activo = true };
			_productoService.Setup(p => p.ObtenerTodos()).Returns(new List<Item> { item });
			_insumoService
				.Setup(i => i.ObtenerMaximoDisponible(It.Is<Item>(x => x.Id == 1), It.IsAny<IReadOnlyDictionary<string, Insumo>>()))
				.Returns(2);
			var pedido = new Pedido
			{
				Mesa = "5",
				Items = new List<ItemPedido> { new() { ItemId = 1, Nombre = "Taco", Cantidad = 5 } }
			};

			// Act
			var resultado = await _useCase.CrearAsync(pedido, "mesero1", "Recepción");

			// Assert
			Assert.Empty(resultado.LineasDescartadas);
			Assert.Single(resultado.LineasAjustadas);
			Assert.Equal(5, resultado.LineasAjustadas[0].CantidadSolicitada);
			Assert.Equal(2, resultado.LineasAjustadas[0].CantidadFinal);
			Assert.Equal(2, resultado.Pedido.Items[0].Cantidad);
		}

		[Fact]
		public async Task CrearAsync_ConStockDeInsumosEnCero_DescartaLaLineaPeroConservaLasDemas()
		{
			// Arrange
			var itemSinStock = new Item { Id = 1, Platillo = "Taco", Precio = 50m, Activo = true };
			var itemConStock = new Item { Id = 2, Platillo = "Refresco", Precio = 30m, Activo = true };
			_productoService.Setup(p => p.ObtenerTodos()).Returns(new List<Item> { itemSinStock, itemConStock });
			_insumoService
				.Setup(i => i.ObtenerMaximoDisponible(It.Is<Item>(x => x.Id == 1), It.IsAny<IReadOnlyDictionary<string, Insumo>>()))
				.Returns(0);
			var pedido = new Pedido
			{
				Mesa = "5",
				Items = new List<ItemPedido>
				{
					new() { ItemId = 1, Nombre = "Taco", Cantidad = 1 },
					new() { ItemId = 2, Nombre = "Refresco", Cantidad = 1 }
				}
			};

			// Act
			var resultado = await _useCase.CrearAsync(pedido, "mesero1", "Recepción");

			// Assert
			Assert.Single(resultado.LineasDescartadas);
			Assert.Equal("Sin stock de ingredientes", resultado.LineasDescartadas[0].Motivo);
			Assert.Single(resultado.Pedido.Items);
		}

		[Fact]
		public async Task CrearAsync_CuandoNingunaLineaEsValida_LanzaExcepcionSinGuardarNiNotificar()
		{
			// Arrange
			_productoService.Setup(p => p.ObtenerTodos()).Returns(new List<Item>());
			var pedido = new Pedido
			{
				Mesa = "5",
				Items = new List<ItemPedido> { new() { ItemId = 1, Nombre = "Fantasma", Cantidad = 1 } }
			};

			// Act & Assert
			await Assert.ThrowsAsync<InvalidOperationException>(() => _useCase.CrearAsync(pedido, "mesero1", "Recepción"));

			_repository.Verify(r => r.GuardarAsync(It.IsAny<Pedido>()), Times.Never);
			_notificador.Verify(n => n.NotificarCreadoAsync(It.IsAny<Pedido>()), Times.Never);
		}

		[Fact]
		public async Task CrearAsync_ConPromocionVigenteParaElItem_AplicaElPrecioConDescuentoALaLinea()
		{
			// Arrange
			var item = new Item { Id = 1, Platillo = "Taco", Precio = 100m, Activo = true };
			_productoService.Setup(p => p.ObtenerTodos()).Returns(new List<Item> { item });
			_promocionService.Setup(p => p.ObtenerVigentes()).Returns(new List<Promocion>
			{
				new() { Id = 1, ItemIds = new List<int> { 1 }, TipoDescuento = TipoDescuento.Porcentaje, ValorDescuento = 10m }
			});
			var pedido = new Pedido
			{
				Mesa = "5",
				Items = new List<ItemPedido> { new() { ItemId = 1, Nombre = "Taco", Cantidad = 1 } }
			};

			// Act
			var resultado = await _useCase.CrearAsync(pedido, "mesero1", "Recepción");

			// Assert
			Assert.Equal(90m, resultado.Pedido.Items[0].PrecioUnitario);
		}

		[Fact]
		public async Task CrearAsync_ConDatosValidos_GuardaConEstadoPendienteYRegistraAuditoria()
		{
			// Arrange
			var item = new Item { Id = 1, Platillo = "Taco", Precio = 50m, Activo = true };
			_productoService.Setup(p => p.ObtenerTodos()).Returns(new List<Item> { item });
			var pedido = new Pedido
			{
				Mesa = "7",
				Items = new List<ItemPedido> { new() { ItemId = 1, Nombre = "Taco", Cantidad = 2 } }
			};

			// Act
			var resultado = await _useCase.CrearAsync(pedido, "mesero1", "Recepción");

			// Assert
			Assert.Equal(EstadoPedido.Pendiente, resultado.Pedido.Estado);
			_repository.Verify(r => r.GuardarAsync(It.IsAny<Pedido>()), Times.Once);
			_notificador.Verify(n => n.NotificarCreadoAsync(resultado.Pedido), Times.Once);
			_auditoriaService.Verify(a => a.RegistrarAccion(
				"mesero1", "Recepción", TipoAccionAuditoria.Creacion, It.IsAny<string>(),
				It.IsAny<string?>(), It.IsAny<string?>()), Times.Once);
		}

		[Fact]
		public async Task CrearAsync_CuandoFallaLaNotificacion_NoInterrumpeLaCreacionDelPedido()
		{
			// Arrange
			var item = new Item { Id = 1, Platillo = "Taco", Precio = 50m, Activo = true };
			_productoService.Setup(p => p.ObtenerTodos()).Returns(new List<Item> { item });
			_notificador.Setup(n => n.NotificarCreadoAsync(It.IsAny<Pedido>())).ThrowsAsync(new Exception("Hub caído"));
			var pedido = new Pedido
			{
				Mesa = "7",
				Items = new List<ItemPedido> { new() { ItemId = 1, Nombre = "Taco", Cantidad = 1 } }
			};

			// Act
			var resultado = await _useCase.CrearAsync(pedido, "mesero1", "Recepción");

			// Assert
			Assert.NotNull(resultado.Pedido);
			_repository.Verify(r => r.GuardarAsync(It.IsAny<Pedido>()), Times.Once);
		}
	}
}
