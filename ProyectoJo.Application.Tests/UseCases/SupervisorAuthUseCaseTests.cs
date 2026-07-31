using Moq;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Application.UseCases;
using ProyectoJo.Domain.Entities;
using Xunit;

namespace ProyectoJo.Application.Tests.UseCases
{
	public class SupervisorAuthUseCaseTests
	{
		private readonly Mock<IAdministradorRepository> _repository = new();
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
			_repository.Verify(r => r.ObtenerTodosAsync(), Times.Never);
		}

		[Fact]
		public async Task ValidarClaveAsync_SinNingunAdministradorConClaveConfigurada_DevuelveFalse()
		{
			_repository.Setup(r => r.ObtenerTodosAsync()).ReturnsAsync(new List<Administrador>
			{
				new() { Id = 1, Usuario = "admin", Activo = true, ClaveSupervisorHash = null }
			});

			var resultado = await _useCase.ValidarClaveAsync("cualquierClave");

			Assert.False(resultado);
		}

		[Fact]
		public async Task ValidarClaveAsync_ConClaveCorrectaDeUnAdministradorActivo_DevuelveTrue()
		{
			var hash = AdministradorUseCase.HashearContrasena("clave123");
			_repository.Setup(r => r.ObtenerTodosAsync()).ReturnsAsync(new List<Administrador>
			{
				new() { Id = 1, Usuario = "admin1", Activo = true, ClaveSupervisorHash = hash }
			});

			var resultado = await _useCase.ValidarClaveAsync("clave123");

			Assert.True(resultado);
		}

		[Fact]
		public async Task ValidarClaveAsync_ConClaveDeUnAdministradorInactivo_DevuelveFalse()
		{
			var hash = AdministradorUseCase.HashearContrasena("clave123");
			_repository.Setup(r => r.ObtenerTodosAsync()).ReturnsAsync(new List<Administrador>
			{
				new() { Id = 1, Usuario = "admin1", Activo = false, ClaveSupervisorHash = hash }
			});

			var resultado = await _useCase.ValidarClaveAsync("clave123");

			Assert.False(resultado);
		}

		[Fact]
		public async Task ValidarClaveAsync_ConClaveIncorrecta_DevuelveFalse()
		{
			var hash = AdministradorUseCase.HashearContrasena("claveCorrecta");
			_repository.Setup(r => r.ObtenerTodosAsync()).ReturnsAsync(new List<Administrador>
			{
				new() { Id = 1, Usuario = "admin1", Activo = true, ClaveSupervisorHash = hash }
			});

			var resultado = await _useCase.ValidarClaveAsync("claveIncorrecta");

			Assert.False(resultado);
		}
	}
}
