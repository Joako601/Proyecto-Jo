using System.Timers;
using Moq;
using ProyectoJo.Application.Ports.In;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Application.UseCases;
using ProyectoJo.Domain.Entities;
using Xunit;

namespace ProyectoJo.Application.Tests.UseCases
{
	public class PromocionUseCaseTests
	{
		private readonly Mock<IPromocionRepository> _repository = new();
		private readonly Mock<IProductoRepository> _productoRepository = new();
		private readonly Mock<IAuditoriaService> _auditoriaService = new();
		private readonly PromocionUseCase _useCase;

		public PromocionUseCaseTests()
		{
			_productoRepository.Setup(r => r.ObtenerTodos()).Returns(new List<Item>());
			_useCase = new PromocionUseCase(_repository.Object, _productoRepository.Object, _auditoriaService.Object);
		}

		[Fact]
		public void Editar_CuandoLaPromocionNoExiste_DevuelveFalseYNoRegistraAuditoria()
		{
			// Arrange
			_repository.Setup(r => r.ObtenerPorId(999)).Returns((Promocion?)null);
			var promoInexistente = new Promocion { Id = 999, Titulo = "Fantasma" };

			// Act
			var resultado = _useCase.Editar(promoInexistente, "admin");

			// Assert
			Assert.False(resultado);
			_repository.Verify(r => r.Editar(It.IsAny<Promocion>()), Times.Never);
			_auditoriaService.Verify(a => a.RegistrarAccion(
				It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TipoAccionAuditoria>(), It.IsAny<string>(),
				It.IsAny<string?>(), It.IsAny<string?>()), Times.Never);
		}

		[Fact]
		public void Editar_CuandoLaPromocionExiste_DevuelveTrueYRegistraAuditoria()
		{
			// Arrange
			var anterior = new Promocion { Id = 1, Titulo = "2x1 Tacos" };
			_repository.Setup(r => r.ObtenerPorId(1)).Returns(anterior);
			var promoEditada = new Promocion { Id = 1, Titulo = "2x1 Tacos Finde" };
			_repository.Setup(r => r.Editar(promoEditada)).Returns(true);

			// Act
			var resultado = _useCase.Editar(promoEditada, "admin");

			// Assert
			Assert.True(resultado);
			_repository.Verify(r => r.Editar(promoEditada), Times.Once);
			_auditoriaService.Verify(a => a.RegistrarAccion(
				"admin", "Promociones", TipoAccionAuditoria.Edicion, It.IsAny<string>(),
				It.IsAny<string?>(), It.IsAny<string?>()), Times.Once);
		}

		[Fact]
		public void Eliminar_CuandoLaPromocionNoExiste_DevuelveFalseYNoRegistraAuditoria()
		{
			// Arrange
			_repository.Setup(r => r.ObtenerPorId(999)).Returns((Promocion?)null);

			// Act
			var resultado = _useCase.Eliminar(999, "admin");

			// Assert
			Assert.False(resultado);
			_repository.Verify(r => r.Eliminar(It.IsAny<int>()), Times.Never);
			_auditoriaService.Verify(a => a.RegistrarAccion(
				It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TipoAccionAuditoria>(), It.IsAny<string>(),
				It.IsAny<string?>(), It.IsAny<string?>()), Times.Never);
		}

		[Fact]
		public void Eliminar_CuandoLaPromocionExiste_DevuelveTrueYRegistraAuditoria()
		{
			// Arrange
			var promocion = new Promocion { Id = 1, Titulo = "2x1 Tacos" };
			_repository.Setup(r => r.ObtenerPorId(1)).Returns(promocion);
			_repository.Setup(r => r.Eliminar(1)).Returns(true);

			// Act
			var resultado = _useCase.Eliminar(1, "admin");

			// Assert
			Assert.True(resultado);
			_repository.Verify(r => r.Eliminar(1), Times.Once);
			_auditoriaService.Verify(a => a.RegistrarAccion(
				"admin", "Promociones", TipoAccionAuditoria.Eliminacion, It.IsAny<string>(),
				It.IsAny<string?>(), It.IsAny<string?>()), Times.Once);
		}

		[Fact]
		public void Agregar_ConItemIdsInexistentes_LosDescartaAntesDeGuardar()
		{
			// Arrange
			_productoRepository.Setup(r => r.ObtenerTodos()).Returns(new List<Item> { new() { Id = 1 }, new() { Id = 2 } });
			var promocion = new Promocion { Titulo = "2x1 Tacos", ItemIds = new List<int> { 1, 2, 999 } };

			// Act
			_useCase.Agregar(promocion, "admin");

			// Assert
			_repository.Verify(r => r.Agregar(It.Is<Promocion>(p => p.ItemIds.SequenceEqual(new[] { 1, 2 }))), Times.Once);
		}

		[Fact]
		public void Editar_ConItemIdsInexistentes_LosDescartaAntesDeGuardar()
		{
			// Arrange
			var anterior = new Promocion { Id = 1, Titulo = "2x1 Tacos" };
			_repository.Setup(r => r.ObtenerPorId(1)).Returns(anterior);
			_repository.Setup(r => r.Editar(It.IsAny<Promocion>())).Returns(true);
			_productoRepository.Setup(r => r.ObtenerTodos()).Returns(new List<Item> { new() { Id = 5 } });
			var promocionEditada = new Promocion { Id = 1, Titulo = "2x1 Tacos", ItemIds = new List<int> { 5, 6 } };

			// Act
			_useCase.Editar(promocionEditada, "admin");

			// Assert
			_repository.Verify(r => r.Editar(It.Is<Promocion>(p => p.ItemIds.SequenceEqual(new[] { 5 }))), Times.Once);
		}

		[Fact]
		public void ActualizarFecha_ConFechaInicioPosteriorAFechaFin_LanzaExcepcionSinConsultarElRepositorio()
		{
			// Arrange
			var fechaInicio = new DateTime(2026, 8, 10);
			var fechaFin = new DateTime(2026, 8, 1);

			// Act & Assert
			Assert.Throws<InvalidOperationException>(() => _useCase.ActualizarFecha(1, fechaInicio, fechaFin, "admin"));
			_repository.Verify(r => r.ObtenerPorId(It.IsAny<int>()), Times.Never);
		}

		[Fact]
		public void ActualizarFecha_ConRangoValido_ActualizaYRegistraAuditoria()
		{
			// Arrange
			var promocion = new Promocion { Id = 1, Titulo = "2x1 Tacos" };
			_repository.Setup(r => r.ObtenerPorId(1)).Returns(promocion);
			_repository.Setup(r => r.Editar(promocion)).Returns(true);
			var fechaInicio = new DateTime(2026, 8, 1);
			var fechaFin = new DateTime(2026, 8, 10);

			// Act
			var resultado = _useCase.ActualizarFecha(1, fechaInicio, fechaFin, "admin");

			// Assert
			Assert.True(resultado);
			Assert.Equal(fechaInicio, promocion.FechaInicio);
			Assert.Equal(fechaFin, promocion.FechaFin);
			_auditoriaService.Verify(a => a.RegistrarAccion(
				"admin", "Promociones", TipoAccionAuditoria.Edicion, It.IsAny<string>(),
				It.IsAny<string?>(), It.IsAny<string?>()), Times.Once);
		}

		[Fact]
		public void ActualizarFecha_CuandoLaPromocionNoExiste_DevuelveFalse()
		{
			// Arrange
			_repository.Setup(r => r.ObtenerPorId(999)).Returns((Promocion?)null);

			// Act
			var resultado = _useCase.ActualizarFecha(999, null, null, "admin");

			// Assert
			Assert.False(resultado);
		}

		[Fact]
		public void EstaVigente_ActivaSinFechas_EsVigente()
		{
			var promocion = new Promocion { Activa = true };

			Assert.True(_useCase.EstaVigente(promocion));
		}

		[Fact]
		public void EstaVigente_Inactiva_NoEsVigenteSinImportarLasFechas()
		{
			var promocion = new Promocion { Activa = false, FechaInicio = DateTime.Today.AddDays(-5), FechaFin = DateTime.Today.AddDays(5) };

			Assert.False(_useCase.EstaVigente(promocion));
		}

		[Fact]
		public void EstaVigente_ConFechaInicioEnElFuturo_NoEsVigente()
		{
			var promocion = new Promocion { Activa = true, FechaInicio = DateTime.Today.AddDays(1) };

			Assert.False(_useCase.EstaVigente(promocion));
		}

		[Fact]
		public void EstaVigente_ConFechaFinEnElPasado_NoEsVigente()
		{
			var promocion = new Promocion { Activa = true, FechaFin = DateTime.Today.AddDays(-1) };

			Assert.False(_useCase.EstaVigente(promocion));
		}

		[Fact]
		public void EstaVigente_DentroDelRangoDeFechas_EsVigente()
		{
			var promocion = new Promocion { Activa = true, FechaInicio = DateTime.Today.AddDays(-1), FechaFin = DateTime.Today.AddDays(1) };

			Assert.True(_useCase.EstaVigente(promocion));
		}

		[Fact]
		public void CalcularPrecioFinal_SinPromocionesQueApliquenAlItem_DevuelveElPrecioOriginal()
		{
			// Arrange: la promoción existe pero es para otro item
			var item = new Item { Id = 1, Precio = 100m };
			var promos = new List<Promocion> { new() { Id = 1, ItemIds = new List<int> { 2 }, TipoDescuento = TipoDescuento.Porcentaje, ValorDescuento = 10m } };

			// Act
			var precio = _useCase.CalcularPrecioFinal(item, promos);

			// Assert
			Assert.Equal(100m, precio);
		}

		[Fact]
		public void CalcularPrecioFinal_ConDescuentoPorcentual_DescuentaElPorcentajeDelPrecio()
		{
			// Arrange
			var item = new Item { Id = 1, Precio = 100m };
			var promos = new List<Promocion> { new() { Id = 1, ItemIds = new List<int> { 1 }, TipoDescuento = TipoDescuento.Porcentaje, ValorDescuento = 20m } };

			// Act
			var precio = _useCase.CalcularPrecioFinal(item, promos);

			// Assert
			Assert.Equal(80m, precio);
		}

		[Fact]
		public void CalcularPrecioFinal_ConDescuentoDeMontoFijo_RestaElMontoDirectamente()
		{
			// Arrange
			var item = new Item { Id = 1, Precio = 100m };
			var promos = new List<Promocion> { new() { Id = 1, ItemIds = new List<int> { 1 }, TipoDescuento = TipoDescuento.MontoFijo, ValorDescuento = 35m } };

			// Act
			var precio = _useCase.CalcularPrecioFinal(item, promos);

			// Assert
			Assert.Equal(65m, precio);
		}

		[Fact]
		public void CalcularPrecioFinal_ConDescuentoMayorAlPrecio_NoDevuelveUnPrecioNegativo()
		{
			// Arrange
			var item = new Item { Id = 1, Precio = 30m };
			var promos = new List<Promocion> { new() { Id = 1, ItemIds = new List<int> { 1 }, TipoDescuento = TipoDescuento.MontoFijo, ValorDescuento = 50m } };

			// Act
			var precio = _useCase.CalcularPrecioFinal(item, promos);

			// Assert
			Assert.Equal(0m, precio);
		}

		[Fact]
		public void CalcularPrecioFinal_ConVariasPromocionesVigentesParaElMismoItem_UsaLaDeIdMasAlto()
		{
			// Arrange
			var item = new Item { Id = 1, Precio = 100m };
			var promos = new List<Promocion>
			{
				new() { Id = 1, ItemIds = new List<int> { 1 }, TipoDescuento = TipoDescuento.Porcentaje, ValorDescuento = 10m },
				new() { Id = 2, ItemIds = new List<int> { 1 }, TipoDescuento = TipoDescuento.Porcentaje, ValorDescuento = 50m }
			};

			// Act
			var precio = _useCase.CalcularPrecioFinal(item, promos);

			// Assert
			Assert.Equal(50m, precio);
		}

		[Fact]
		public void CalcularPrecioFinal_RedondeaElResultadoADosDecimales()
		{
			// Arrange: 19.99 - 10% = 17.991, debe redondear a 17.99
			var item = new Item { Id = 1, Precio = 19.99m };
			var promos = new List<Promocion> { new() { Id = 1, ItemIds = new List<int> { 1 }, TipoDescuento = TipoDescuento.Porcentaje, ValorDescuento = 10m } };

			// Act
			var precio = _useCase.CalcularPrecioFinal(item, promos);

			// Assert
			Assert.Equal(17.99m, precio);
		}

		[Fact]
		public void CalcularPrecioFinal_ConUnSoloArgumento_ObtieneLasPromocionesVigentesDelRepositorio()
		{
			// Arrange
			var item = new Item { Id = 1, Precio = 100m };
			var promoVigente = new Promocion { Id = 1, Activa = true, ItemIds = new List<int> { 1 }, TipoDescuento = TipoDescuento.MontoFijo, ValorDescuento = 25m };
			_repository.Setup(r => r.ObtenerTodas()).Returns(new List<Promocion> { promoVigente });

			// Act
			var precio = _useCase.CalcularPrecioFinal(item);

			// Assert
			Assert.Equal(75m, precio);
		}
	}
}