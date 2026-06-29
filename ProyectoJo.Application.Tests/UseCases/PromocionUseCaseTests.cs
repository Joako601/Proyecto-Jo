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
		private readonly Mock<IAuditoriaService> _auditoriaService = new();
		private readonly PromocionUseCase _useCase;

		public PromocionUseCaseTests()
		{
			_useCase = new PromocionUseCase(_repository.Object, _auditoriaService.Object);
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

			// Act
			var resultado = _useCase.Eliminar(1, "admin");

			// Assert
			Assert.True(resultado);
			_repository.Verify(r => r.Eliminar(1), Times.Once);
			_auditoriaService.Verify(a => a.RegistrarAccion(
				"admin", "Promociones", TipoAccionAuditoria.Eliminacion, It.IsAny<string>(),
				It.IsAny<string?>(), It.IsAny<string?>()), Times.Once);
		}
	}
}