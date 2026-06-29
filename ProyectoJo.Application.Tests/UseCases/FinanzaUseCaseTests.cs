using System.Timers;
using Moq;
using ProyectoJo.Application.Ports.In;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Application.UseCases;
using ProyectoJo.Domain.Entities;
using Xunit;

namespace ProyectoJo.Application.Tests.UseCases
{
	public class FinanzaUseCaseTests
	{
		private readonly Mock<IFinanzaRepository> _repository = new();
		private readonly Mock<IAuditoriaService> _auditoriaService = new();
		private readonly FinanzaUseCase _useCase;

		public FinanzaUseCaseTests()
		{
			_useCase = new FinanzaUseCase(_repository.Object, _auditoriaService.Object);
		}

		[Fact]
		public void Editar_CuandoLaFinanzaNoExiste_DevuelveFalseYNoRegistraAuditoria()
		{
			// Arrange
			_repository.Setup(r => r.ObtenerPorId(999)).Returns((Finanza?)null);
			var finanzaInexistente = new Finanza { Id = 999, Monto = 100, Categoria = "Ventas", Descripcion = "x", Tipo = TipoMovimiento.Ingreso };

			// Act
			var resultado = _useCase.Editar(finanzaInexistente, "admin");

			// Assert
			Assert.False(resultado);
			_repository.Verify(r => r.Actualizar(It.IsAny<Finanza>()), Times.Never);
			_auditoriaService.Verify(a => a.RegistrarAccion(
				It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TipoAccionAuditoria>(), It.IsAny<string>(),
				It.IsAny<string?>(), It.IsAny<string?>()), Times.Never);
		}

		[Fact]
		public void Editar_CuandoLaFinanzaExiste_DevuelveTrueYRegistraAuditoria()
		{
			// Arrange
			var anterior = new Finanza { Id = 1, Monto = 100, Categoria = "Ventas", Descripcion = "x", Tipo = TipoMovimiento.Ingreso };
			_repository.Setup(r => r.ObtenerPorId(1)).Returns(anterior);
			var finanzaEditada = new Finanza { Id = 1, Monto = 150, Categoria = "Ventas", Descripcion = "y", Tipo = TipoMovimiento.Ingreso };

			// Act
			var resultado = _useCase.Editar(finanzaEditada, "admin");

			// Assert
			Assert.True(resultado);
			_repository.Verify(r => r.Actualizar(finanzaEditada), Times.Once);
			_auditoriaService.Verify(a => a.RegistrarAccion(
				"admin", "Finanzas", TipoAccionAuditoria.Edicion, It.IsAny<string>(),
				It.IsAny<string?>(), It.IsAny<string?>()), Times.Once);
		}

		[Fact]
		public void Eliminar_CuandoLaFinanzaNoExiste_DevuelveFalseYNoRegistraAuditoria()
		{
			// Arrange
			_repository.Setup(r => r.ObtenerPorId(999)).Returns((Finanza?)null);

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
		public void Eliminar_CuandoLaFinanzaExiste_DevuelveTrueYRegistraAuditoria()
		{
			// Arrange
			var finanza = new Finanza { Id = 1, Monto = 100, Categoria = "Ventas", Descripcion = "x", Tipo = TipoMovimiento.Ingreso };
			_repository.Setup(r => r.ObtenerPorId(1)).Returns(finanza);

			// Act
			var resultado = _useCase.Eliminar(1, "admin");

			// Assert
			Assert.True(resultado);
			_repository.Verify(r => r.Eliminar(1), Times.Once);
			_auditoriaService.Verify(a => a.RegistrarAccion(
				"admin", "Finanzas", TipoAccionAuditoria.Eliminacion, It.IsAny<string>(),
				It.IsAny<string?>(), It.IsAny<string?>()), Times.Once);
		}
	}
}