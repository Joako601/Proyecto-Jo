using Moq;
using ProyectoJo.Application.Ports.In;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Application.UseCases;
using ProyectoJo.Domain.Entities;
using Xunit;

namespace ProyectoJo.Application.Tests.UseCases
{
	public class InsumoUseCaseTests
	{
		private readonly Mock<IInsumoRepository> _repository = new();
		private readonly Mock<IAuditoriaService> _auditoriaService = new();
		private readonly InsumoUseCase _useCase;

		public InsumoUseCaseTests()
		{
			_useCase = new InsumoUseCase(_repository.Object, _auditoriaService.Object);
		}

		[Fact]
		public void Editar_CuandoElInsumoNoExiste_DevuelveFalseYNoRegistraAuditoria()
		{
			_repository.Setup(r => r.ObtenerPorId(999)).Returns((Insumo?)null);

			var resultado = _useCase.Editar(new Insumo { Id = 999 }, "admin");

			Assert.False(resultado);
			_repository.Verify(r => r.Editar(It.IsAny<Insumo>()), Times.Never);
			_auditoriaService.Verify(a => a.RegistrarAccion(
				It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TipoAccionAuditoria>(), It.IsAny<string>(),
				It.IsAny<string?>(), It.IsAny<string?>()), Times.Never);
		}

		[Fact]
		public void Editar_CuandoElInsumoExiste_ActualizaYRegistraAuditoria()
		{
			var anterior = new Insumo { Id = 1, Nombre = "Harina", StockActual = 10, StockMinimo = 2, Unidad = UnidadIngrediente.Kilogramo };
			_repository.Setup(r => r.ObtenerPorId(1)).Returns(anterior);
			var editado = new Insumo { Id = 1, Nombre = "Harina", StockActual = 5, StockMinimo = 2, Unidad = UnidadIngrediente.Kilogramo };
			_repository.Setup(r => r.Editar(editado)).Returns(true);

			var resultado = _useCase.Editar(editado, "admin");

			Assert.True(resultado);
			_auditoriaService.Verify(a => a.RegistrarAccion(
				"admin", "Insumos", TipoAccionAuditoria.Edicion, It.IsAny<string>(),
				It.IsAny<string?>(), It.IsAny<string?>()), Times.Once);
		}

		[Fact]
		public void Eliminar_CuandoElInsumoExiste_EliminaYRegistraAuditoria()
		{
			var insumo = new Insumo { Id = 1, Nombre = "Harina" };
			_repository.Setup(r => r.ObtenerPorId(1)).Returns(insumo);
			_repository.Setup(r => r.Eliminar(1)).Returns(true);

			var resultado = _useCase.Eliminar(1, "admin");

			Assert.True(resultado);
			_auditoriaService.Verify(a => a.RegistrarAccion(
				"admin", "Insumos", TipoAccionAuditoria.Eliminacion, It.IsAny<string>(),
				It.IsAny<string?>(), It.IsAny<string?>()), Times.Once);
		}

		[Fact]
		public async Task ReponerAsync_ConCantidadNoPositiva_DevuelveCantidadInvalidaSinConsultarElRepositorio()
		{
			var resultado = await _useCase.ReponerAsync(1, 0, "admin");

			Assert.Equal(ResultadoReponerInsumo.CantidadInvalida, resultado);
			_repository.Verify(r => r.ObtenerPorId(It.IsAny<int>()), Times.Never);
		}

		[Fact]
		public async Task ReponerAsync_CuandoElInsumoNoExiste_DevuelveInsumoNoEncontradoSinLlamarAlRepositorioAtomico()
		{
			_repository.Setup(r => r.ObtenerPorId(999)).Returns((Insumo?)null);

			var resultado = await _useCase.ReponerAsync(999, 10, "admin");

			Assert.Equal(ResultadoReponerInsumo.InsumoNoEncontrado, resultado);
			_repository.Verify(r => r.ReponerAtomicoAsync(It.IsAny<int>(), It.IsAny<decimal>()), Times.Never);
		}

		[Fact]
		public async Task ReponerAsync_CuandoSePierdeLaCarreraAtomica_DevuelveConflictoDeConcurrencia()
		{
			var anterior = new Insumo { Id = 1, Nombre = "Harina", StockActual = 5, Unidad = UnidadIngrediente.Kilogramo };
			_repository.Setup(r => r.ObtenerPorId(1)).Returns(anterior);
			_repository.Setup(r => r.ReponerAtomicoAsync(1, 10)).ReturnsAsync((Insumo?)null);

			var resultado = await _useCase.ReponerAsync(1, 10, "admin");

			Assert.Equal(ResultadoReponerInsumo.ConflictoDeConcurrencia, resultado);
			_auditoriaService.Verify(a => a.RegistrarAccion(
				It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TipoAccionAuditoria>(), It.IsAny<string>(),
				It.IsAny<string?>(), It.IsAny<string?>()), Times.Never);
		}

		[Fact]
		public async Task ReponerAsync_ConDatosValidos_ActualizaStockYRegistraAuditoria()
		{
			var anterior = new Insumo { Id = 1, Nombre = "Harina", StockActual = 5, Unidad = UnidadIngrediente.Kilogramo };
			var actualizado = new Insumo { Id = 1, Nombre = "Harina", StockActual = 15, Unidad = UnidadIngrediente.Kilogramo };
			_repository.Setup(r => r.ObtenerPorId(1)).Returns(anterior);
			_repository.Setup(r => r.ReponerAtomicoAsync(1, 10)).ReturnsAsync(actualizado);

			var resultado = await _useCase.ReponerAsync(1, 10, "admin");

			Assert.Equal(ResultadoReponerInsumo.Exitoso, resultado);
			_auditoriaService.Verify(a => a.RegistrarAccion(
				"admin", "Insumos", TipoAccionAuditoria.Edicion, It.IsAny<string>(),
				It.IsAny<string?>(), It.IsAny<string?>()), Times.Once);
		}

		[Fact]
		public async Task VerificarYDescontarAsync_CuandoNingunItemTieneReceta_DevuelveNullSinLlamarAlRepositorio()
		{
			var items = new List<ItemPedido> { new() { ItemId = 1, Cantidad = 2 } };

			var resultado = await _useCase.VerificarYDescontarAsync(items, _ => null);

			Assert.Null(resultado);
			_repository.Verify(r => r.DescontarAtomicoAsync(It.IsAny<Dictionary<int, decimal>>()), Times.Never);
		}

		[Fact]
		public async Task VerificarYDescontarAsync_ConStockSuficiente_DevuelveNull()
		{
			var receta = new Receta
			{
				Id = 1,
				ItemId = 1,
				Ingredientes = new List<IngredienteReceta> { new() { InsumoId = 10, Cantidad = 2 } }
			};
			var items = new List<ItemPedido> { new() { ItemId = 1, Cantidad = 3 } };

			_repository
				.Setup(r => r.DescontarAtomicoAsync(It.Is<Dictionary<int, decimal>>(d => d[10] == 6)))
				.ReturnsAsync((true, new List<FaltanteInsumo>()));

			var resultado = await _useCase.VerificarYDescontarAsync(items, _ => receta);

			Assert.Null(resultado);
		}

		[Fact]
		public async Task VerificarYDescontarAsync_ConStockInsuficiente_DevuelveMensajeConElDetalle()
		{
			var receta = new Receta
			{
				Id = 1,
				ItemId = 1,
				Ingredientes = new List<IngredienteReceta> { new() { InsumoId = 10, Cantidad = 2 } }
			};
			var items = new List<ItemPedido> { new() { ItemId = 1, Cantidad = 3 } };
			var faltantes = new List<FaltanteInsumo> { new() { InsumoId = 10, Nombre = "Harina", Necesario = 6, Disponible = 2 } };

			_repository
				.Setup(r => r.DescontarAtomicoAsync(It.IsAny<Dictionary<int, decimal>>()))
				.ReturnsAsync((false, faltantes));

			var resultado = await _useCase.VerificarYDescontarAsync(items, _ => receta);

			Assert.NotNull(resultado);
			Assert.Contains("Harina", resultado);
		}

		[Fact]
		public void ObtenerMaximoDisponible_TomaElMinimoStockEntreLosIngredientes()
		{
			var item = new Item { Id = 1, Ingredientes = "Harina, Queso" };
			var insumos = new Dictionary<string, Insumo>(StringComparer.OrdinalIgnoreCase)
			{
				["Harina"] = new Insumo { Nombre = "Harina", StockActual = 10 },
				["Queso"] = new Insumo { Nombre = "Queso", StockActual = 3 }
			};

			var maximo = _useCase.ObtenerMaximoDisponible(item, insumos);

			Assert.Equal(3, maximo);
		}
	}
}
