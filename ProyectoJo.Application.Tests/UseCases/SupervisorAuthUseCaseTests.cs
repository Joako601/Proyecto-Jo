using Moq;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Application.UseCases;
using Xunit;

namespace ProyectoJo.Application.Tests.UseCases
{
	public class SupervisorAuthUseCaseTests
	{
		private readonly Mock<ISupervisorClaveRepository> _repository = new();
		private readonly SupervisorAuthUseCase _useCase;

		public SupervisorAuthUseCaseTests()
		{
			_useCase = new SupervisorAuthUseCase(_repository.Object);
		}

		[Fact]
		public async Task ValidarClaveAsync_ConClaveVacia_DevuelveFalseSinConsultarElRepositorio()
		{
			var resultado = await _useCase.ValidarClaveAsync("");

			Assert.False(resultado);
			_repository.Verify(r => r.ObtenerHashAsync(), Times.Never);
		}

		[Fact]
		public async Task ValidarClaveAsync_SinClaveConfigurada_DevuelveFalse()
		{
			_repository.Setup(r => r.ObtenerHashAsync()).ReturnsAsync((string?)null);

			var resultado = await _useCase.ValidarClaveAsync("cualquierClave");

			Assert.False(resultado);
		}

		[Fact]
		public async Task TieneClaveConfiguradaAsync_DevuelveTrueSoloSiHayHashGuardado()
		{
			_repository.Setup(r => r.ObtenerHashAsync()).ReturnsAsync((string?)null);
			Assert.False(await _useCase.TieneClaveConfiguradaAsync());

			_repository.Setup(r => r.ObtenerHashAsync()).ReturnsAsync("hash-existente");
			Assert.True(await _useCase.TieneClaveConfiguradaAsync());
		}

		[Fact]
		public async Task CambiarClaveAsync_ConNuevaClaveCorta_DevuelveFalseSinGuardar()
		{
			var resultado = await _useCase.CambiarClaveAsync(null, "123");

			Assert.False(resultado);
			_repository.Verify(r => r.GuardarHashAsync(It.IsAny<string>()), Times.Never);
		}

		[Fact]
		public async Task CambiarClaveAsync_SinClaveConfigurada_PermiteEstablecerlaSinClaveActual()
		{
			_repository.Setup(r => r.ObtenerHashAsync()).ReturnsAsync((string?)null);

			var resultado = await _useCase.CambiarClaveAsync(null, "claveNueva123");

			Assert.True(resultado);
			_repository.Verify(r => r.GuardarHashAsync(It.Is<string>(h => h.Contains('.'))), Times.Once);
		}

		[Fact]
		public async Task CambiarClaveAsync_ConClaveActualIncorrecta_DevuelveFalseSinGuardar()
		{
			_repository.Setup(r => r.ObtenerHashAsync()).ReturnsAsync("saltInvalido.hashInvalido");

			var resultado = await _useCase.CambiarClaveAsync("claveIncorrecta", "claveNueva123");

			Assert.False(resultado);
			_repository.Verify(r => r.GuardarHashAsync(It.IsAny<string>()), Times.Never);
		}

		[Fact]
		public async Task CambiarClaveAsync_ConClaveActualCorrecta_ActualizaElHash()
		{
			string? hashGuardado = null;
			_repository.Setup(r => r.GuardarHashAsync(It.IsAny<string>()))
				.Callback<string>(h => hashGuardado = h)
				.Returns(Task.CompletedTask);
			_repository.Setup(r => r.ObtenerHashAsync()).ReturnsAsync(() => hashGuardado);

			// Primero se establece la clave inicial (todavía no hay ninguna configurada)
			await _useCase.CambiarClaveAsync(null, "claveInicial1");

			// Luego se cambia usando la clave actual correcta
			var resultado = await _useCase.CambiarClaveAsync("claveInicial1", "claveNueva123");

			Assert.True(resultado);

			var validaConLaAnterior = await _useCase.ValidarClaveAsync("claveInicial1");
			Assert.False(validaConLaAnterior);

			var validaConLaNueva = await _useCase.ValidarClaveAsync("claveNueva123");
			Assert.True(validaConLaNueva);
		}
	}
}
