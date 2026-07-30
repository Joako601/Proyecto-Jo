using System.Timers;
using Moq;
using ProyectoJo.Application.Ports.In;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Application.UseCases;
using ProyectoJo.Domain.Entities;
using Xunit;

namespace ProyectoJo.Application.Tests.UseCases
{
	public class ProductoUseCaseTests
	{
		private readonly Mock<IProductoRepository> _repository = new();
		private readonly Mock<IAuditoriaService> _auditoriaService = new();
		private readonly ProductoUseCase _useCase;

		public ProductoUseCaseTests()
		{
			_useCase = new ProductoUseCase(_repository.Object, _auditoriaService.Object);
		}

		[Fact]
		public void EditarItem_CuandoElItemNoExiste_DevuelveFalseYNoRegistraAuditoria()
		{
			// Arrange: no hay ningún item con ese Id en el repositorio
			_repository.Setup(r => r.ObtenerPorId(999)).Returns((Item?)null);

			var itemInexistente = new Item { Id = 999, Platillo = "Fantasma", Categoria = "Comida", Precio = 10 };

			// Act
			var resultado = _useCase.EditarItem(itemInexistente, "admin");

			// Assert
			Assert.False(resultado);
			_repository.Verify(r => r.ActualizarItem(It.IsAny<Item>()), Times.Never);
			_auditoriaService.Verify(a => a.RegistrarAccion(
				It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TipoAccionAuditoria>(), It.IsAny<string>(),
				It.IsAny<string?>(), It.IsAny<string?>()), Times.Never);
		}

		[Fact]
		public void EditarItem_CuandoElItemExiste_DevuelveTrueYRegistraAuditoria()
		{
			// Arrange
			var anterior = new Item { Id = 1, Platillo = "Tacos", Categoria = "Comida", Precio = 50 };
			_repository.Setup(r => r.ObtenerPorId(1)).Returns(anterior);

			var itemEditado = new Item { Id = 1, Platillo = "Tacos al Pastor", Categoria = "Comida", Precio = 60 };

			// Act
			var resultado = _useCase.EditarItem(itemEditado, "admin");

			// Assert
			Assert.True(resultado);
			_repository.Verify(r => r.ActualizarItem(It.Is<Item>(
				i => i.Id == 1 && i.Platillo == "Tacos al Pastor")), Times.Once);
			_auditoriaService.Verify(a => a.RegistrarAccion(
				"admin", "Productos", TipoAccionAuditoria.Edicion, It.IsAny<string>(),
				It.IsAny<string?>(), It.IsAny<string?>()), Times.Once);
		}

		[Fact]
		public void Eliminar_CuandoElItemNoExiste_DevuelveFalseYNoRegistraAuditoria()
		{
			// Arrange
			_repository.Setup(r => r.ObtenerPorId(999)).Returns((Item?)null);

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
		public void Eliminar_CuandoElItemExiste_DevuelveTrueYRegistraAuditoria()
		{
			// Arrange
			var item = new Item { Id = 1, Platillo = "Tacos", Categoria = "Comida", Precio = 50 };
			_repository.Setup(r => r.ObtenerPorId(1)).Returns(item);
			_repository.Setup(r => r.Eliminar(1)).Returns(true);

			// Act
			var resultado = _useCase.Eliminar(1, "admin");

			// Assert
			Assert.True(resultado);
			_repository.Verify(r => r.Eliminar(1), Times.Once);
			_auditoriaService.Verify(a => a.RegistrarAccion(
				"admin", "Productos", TipoAccionAuditoria.Eliminacion, It.IsAny<string>(),
				It.IsAny<string?>(), It.IsAny<string?>()), Times.Once);
		}
	}
}