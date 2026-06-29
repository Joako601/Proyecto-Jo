using Moq;
using ProyectoJo.Application.Ports.In;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Application.UseCases;
using ProyectoJo.Domain.Entities;
using Xunit;

namespace ProyectoJo.Application.Tests.UseCases
{
	public class CierreCajaUseCaseTests
	{
		private readonly Mock<ICierreCajaRepository> _cierreCajaRepository = new();
		private readonly Mock<IFinanzaRepository> _finanzaRepository = new();
		private readonly Mock<IAuditoriaService> _auditoriaService = new();
		private readonly CierreCajaUseCase _useCase;

		public CierreCajaUseCaseTests()
		{
			_useCase = new CierreCajaUseCase(_cierreCajaRepository.Object, _finanzaRepository.Object, _auditoriaService.Object);
		}

		[Fact]
		public void AbrirCaja_CuandoYaHayUnaCajaAbierta_LanzaExcepcionYNoRegistraAuditoria()
		{
			// Arrange: el repositorio simula que ya existe una caja abierta,
			_cierreCajaRepository.Setup(r => r.IntentarAbrir(It.IsAny<CierreCaja>())).Returns(false);

			// Act + Assert
			var excepcion = Assert.Throws<InvalidOperationException>(
				() => _useCase.AbrirCaja(fondoInicial: 500, notas: null, usuario: "admin"));

			Assert.Contains("Ya hay una caja abierta", excepcion.Message);
			_auditoriaService.Verify(a => a.RegistrarAccion(
				It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TipoAccionAuditoria>(), It.IsAny<string>(),
				It.IsAny<string?>(), It.IsAny<string?>()), Times.Never);
		}

		[Fact]
		public void AbrirCaja_CuandoNoHayCajaAbierta_DevuelveLaCajaYRegistraAuditoria()
		{
			// Arrange: el repositorio simula que pudo abrir sin problema
			_cierreCajaRepository
				.Setup(r => r.IntentarAbrir(It.IsAny<CierreCaja>()))
				.Callback<CierreCaja>(c => c.Id = 1)
				.Returns(true);

			// Act
			var resultado = _useCase.AbrirCaja(fondoInicial: 500, notas: "Apertura turno mañana", usuario: "admin");

			// Assert
			Assert.Equal(EstadoCaja.Abierta, resultado.Estado);
			Assert.Equal(500, resultado.FondoInicial);
			_cierreCajaRepository.Verify(r => r.IntentarAbrir(It.IsAny<CierreCaja>()), Times.Once);
			_auditoriaService.Verify(a => a.RegistrarAccion(
				"admin", "CierreCaja", TipoAccionAuditoria.Creacion, It.IsAny<string>(),
				It.IsAny<string?>(), It.IsAny<string?>()), Times.Once);
		}
	}
}