using Moq;
using ProyectoJo.Application.Ports.In;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Application.UseCases;
using ProyectoJo.Domain.Entities;
using Xunit;

namespace ProyectoJo.Application.Tests.UseCases
{
	public class RecetaUseCaseTests
	{
		private readonly Mock<IRecetaRepository> _repository = new();
		private readonly Mock<IProductoService> _productoService = new();
		private readonly Mock<IAuditoriaService> _auditoriaService = new();
		private readonly RecetaUseCase _useCase;

		public RecetaUseCaseTests()
		{
			_useCase = new RecetaUseCase(_repository.Object, _productoService.Object, _auditoriaService.Object);
		}

		[Fact]
		public void Agregar_RegistraAuditoriaConElNombreDelPlatillo()
		{
			var receta = new Receta { Id = 1, ItemId = 5, NombreReceta = "Receta X" };
			_productoService.Setup(p => p.ObtenerPorId(5)).Returns(new Item { Id = 5, Platillo = "Tacos" });

			_useCase.Agregar(receta, "admin");

			_repository.Verify(r => r.Agregar(receta), Times.Once);
			_auditoriaService.Verify(a => a.RegistrarAccion(
				"admin", "Recetario", TipoAccionAuditoria.Creacion,
				It.Is<string>(e => e.Contains("Tacos")),
				It.IsAny<string?>(), It.IsAny<string?>()), Times.Once);
		}

		[Fact]
		public void Editar_CuandoLaRecetaNoExiste_DevuelveFalse()
		{
			_repository.Setup(r => r.ObtenerPorId(999)).Returns((Receta?)null);

			var resultado = _useCase.Editar(new Receta { Id = 999 }, "admin");

			Assert.False(resultado);
			_repository.Verify(r => r.Editar(It.IsAny<Receta>()), Times.Never);
		}

		[Fact]
		public void Eliminar_CuandoLaRecetaExiste_EliminaYRegistraAuditoria()
		{
			var receta = new Receta { Id = 1, ItemId = 5, NombreReceta = "Receta X" };
			_repository.Setup(r => r.ObtenerPorId(1)).Returns(receta);
			_repository.Setup(r => r.Eliminar(1)).Returns(true);
			_productoService.Setup(p => p.ObtenerPorId(5)).Returns((Item?)null);

			var resultado = _useCase.Eliminar(1, "admin");

			Assert.True(resultado);
			_auditoriaService.Verify(a => a.RegistrarAccion(
				"admin", "Recetario", TipoAccionAuditoria.Eliminacion,
				It.Is<string>(e => e.Contains("Receta X")),
				It.IsAny<string?>(), It.IsAny<string?>()), Times.Once);
		}

		[Fact]
		public void ObtenerRendimiento_CuandoLaRecetaNoExiste_DevuelveNull()
		{
			_repository.Setup(r => r.ObtenerPorId(999)).Returns((Receta?)null);

			var resultado = _useCase.ObtenerRendimiento(999);

			Assert.Null(resultado);
		}

		[Fact]
		public void ObtenerRendimiento_ConDatosValidos_CalculaCostoYPrecioDeVenta()
		{
			var receta = new Receta
			{
				Id = 1,
				ItemId = 5,
				Rendimiento = 4,
				Ingredientes = new List<IngredienteReceta> { new() { InsumoId = 1, Cantidad = 2, CostoUnitario = 10 } }
			};
			_repository.Setup(r => r.ObtenerPorId(1)).Returns(receta);
			_productoService.Setup(p => p.ObtenerPorId(5)).Returns(new Item { Id = 5, Platillo = "Tacos", Precio = 50 });

			var resultado = _useCase.ObtenerRendimiento(1);

			Assert.NotNull(resultado);
			Assert.Equal("Tacos", resultado!.Platillo);
			Assert.Equal(20m, resultado.CostoTotal);
			Assert.Equal(5m, resultado.CostoPorPorcion);
			Assert.Equal(50m, resultado.PrecioVenta);
		}

		[Fact]
		public void ObtenerRendimientoDeTodas_OmiteRecetasCuyoProductoYaNoExiste()
		{
			var recetaValida = new Receta { Id = 1, ItemId = 5, Rendimiento = 1 };
			var recetaHuerfana = new Receta { Id = 2, ItemId = 99, Rendimiento = 1 };
			_repository.Setup(r => r.ObtenerTodas()).Returns(new List<Receta> { recetaValida, recetaHuerfana });
			_productoService.Setup(p => p.ObtenerTodos()).Returns(new List<Item> { new() { Id = 5, Platillo = "Tacos", Precio = 50 } });

			var resultado = _useCase.ObtenerRendimientoDeTodas();

			Assert.Single(resultado);
			Assert.Equal(5, resultado[0].ItemId);
		}
	}
}
