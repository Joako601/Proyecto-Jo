using Moq;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Application.UseCases;
using ProyectoJo.Domain.Entities;
using Xunit;

namespace ProyectoJo.Application.Tests.UseCases
{
	public class DispositivoUseCaseTests
	{
		private readonly Mock<IDispositivoRepository> _repository = new();
		private readonly DispositivoUseCase _useCase;

		public DispositivoUseCaseTests()
		{
			_useCase = new DispositivoUseCase(_repository.Object);
		}

		[Fact]
		public async Task EmparejarAsync_GeneraUnTokenYLoRegistra()
		{
			DispositivoOperaciones? pasado = null;
			_repository
				.Setup(r => r.RegistrarAsync(It.IsAny<DispositivoOperaciones>()))
				.Callback<DispositivoOperaciones>(d => pasado = d)
				.ReturnsAsync((DispositivoOperaciones d) => d);

			var resultado = await _useCase.EmparejarAsync(RolEmpleado.Cocina, "Tablet Cocina 1");

			Assert.NotNull(pasado);
			Assert.False(string.IsNullOrWhiteSpace(pasado!.Token));
			Assert.Equal(RolEmpleado.Cocina, pasado.Estacion);
			Assert.Equal("Tablet Cocina 1", pasado.Nombre);
			Assert.Same(pasado, resultado);
		}

		[Fact]
		public async Task ReasignarEstacionAsync_ConTokenVacio_DevuelveNullSinConsultarRepositorio()
		{
			var resultado = await _useCase.ReasignarEstacionAsync("", RolEmpleado.Recepcion, null);

			Assert.Null(resultado);
			_repository.Verify(r => r.ActualizarEstacionAsync(It.IsAny<string>(), It.IsAny<RolEmpleado>(), It.IsAny<string?>()), Times.Never);
		}

		[Fact]
		public async Task ReasignarEstacionAsync_ConTokenValido_DelegaAlRepositorio()
		{
			var dispositivo = new DispositivoOperaciones { Id = 1, Token = "abc", Estacion = RolEmpleado.Recepcion };
			_repository.Setup(r => r.ActualizarEstacionAsync("abc", RolEmpleado.Recepcion, "Caja 1")).ReturnsAsync(dispositivo);

			var resultado = await _useCase.ReasignarEstacionAsync("abc", RolEmpleado.Recepcion, "Caja 1");

			Assert.Same(dispositivo, resultado);
		}

		[Fact]
		public async Task ToggleBloqueadoAsync_DelegaAlRepositorio()
		{
			var dispositivo = new DispositivoOperaciones { Id = 1, Token = "abc", Bloqueado = true };
			_repository.Setup(r => r.ToggleBloqueadoAsync(1)).ReturnsAsync(dispositivo);

			var resultado = await _useCase.ToggleBloqueadoAsync(1);

			Assert.Same(dispositivo, resultado);
		}

		[Fact]
		public async Task ToggleActivoAsync_DelegaAlRepositorio()
		{
			var dispositivo = new DispositivoOperaciones { Id = 1, Token = "abc", Activo = false };
			_repository.Setup(r => r.ToggleActivoAsync(1)).ReturnsAsync(dispositivo);

			var resultado = await _useCase.ToggleActivoAsync(1);

			Assert.Same(dispositivo, resultado);
		}

		[Fact]
		public async Task ReconocerAsync_ConTokenVacio_DevuelveNullSinConsultarRepositorio()
		{
			var resultado = await _useCase.ReconocerAsync(" ");

			Assert.Null(resultado);
			_repository.Verify(r => r.ObtenerPorTokenAsync(It.IsAny<string>()), Times.Never);
		}

		[Fact]
		public async Task ReconocerAsync_ConTokenValido_DelegaAlRepositorio()
		{
			var dispositivo = new DispositivoOperaciones { Id = 1, Token = "abc", Estacion = RolEmpleado.Cocina };
			_repository.Setup(r => r.ObtenerPorTokenAsync("abc")).ReturnsAsync(dispositivo);

			var resultado = await _useCase.ReconocerAsync("abc");

			Assert.Same(dispositivo, resultado);
		}
	}
}
