using Moq;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Application.UseCases;
using ProyectoJo.Domain.Entities;
using Xunit;

namespace ProyectoJo.Application.Tests.UseCases
{
	public class AdministradorUseCaseTests
	{
		private readonly Mock<IAdministradorRepository> _repository = new();
		private readonly AdministradorUseCase _useCase;

		public AdministradorUseCaseTests()
		{
			_useCase = new AdministradorUseCase(_repository.Object);
		}

		[Fact]
		public async Task CrearAsync_ConContrasenaCorta_DevuelveErrorSinAgregar()
		{
			var (exito, error) = await _useCase.CrearAsync("nuevo", "1234567", new List<string>());

			Assert.False(exito);
			Assert.Equal("La contraseña debe tener al menos 8 caracteres.", error);
			_repository.Verify(r => r.AgregarAsync(It.IsAny<Administrador>()), Times.Never);
		}

		[Fact]
		public async Task CrearAsync_ConUsuarioYaExistente_DevuelveError()
		{
			_repository.Setup(r => r.ObtenerPorUsuarioAsync("admin2"))
				.ReturnsAsync(new Administrador { Id = 1, Usuario = "admin2" });

			var (exito, error) = await _useCase.CrearAsync("admin2", "contrasenaValida", new List<string>());

			Assert.False(exito);
			Assert.Equal("Ya existe un administrador con ese usuario.", error);
			_repository.Verify(r => r.AgregarAsync(It.IsAny<Administrador>()), Times.Never);
		}

		[Fact]
		public async Task CrearAsync_ConDatosValidos_HasheaLaContrasenaYFiltraAreasInvalidas()
		{
			_repository.Setup(r => r.ObtenerPorUsuarioAsync("nuevo")).ReturnsAsync((Administrador?)null);

			Administrador? guardado = null;
			_repository.Setup(r => r.AgregarAsync(It.IsAny<Administrador>()))
				.Callback<Administrador>(a => guardado = a)
				.Returns(Task.CompletedTask);

			var (exito, error) = await _useCase.CrearAsync(" nuevo ", "contrasenaValida", new List<string> { "Menu", "AreaInventada" });

			Assert.True(exito);
			Assert.Null(error);
			Assert.NotNull(guardado);
			Assert.Equal("nuevo", guardado!.Usuario);
			Assert.NotEqual("contrasenaValida", guardado.ContrasenaHash);
			Assert.Single(guardado.Areas);
			Assert.Equal("Menu", guardado.Areas[0]);
		}

		[Fact]
		public async Task EditarAsync_CuandoElAdministradorNoExiste_DevuelveError()
		{
			_repository.Setup(r => r.ObtenerPorIdAsync(999)).ReturnsAsync((Administrador?)null);

			var (exito, error) = await _useCase.EditarAsync(999, "usuario", true, null, new List<string>());

			Assert.False(exito);
			Assert.Equal("El administrador no existe.", error);
		}

		[Fact]
		public async Task EditarAsync_ConNuevaContrasenaCorta_DevuelveError()
		{
			var existente = new Administrador { Id = 1, Usuario = "admin", Areas = new List<string>() };
			_repository.Setup(r => r.ObtenerPorIdAsync(1)).ReturnsAsync(existente);
			_repository.Setup(r => r.ObtenerPorUsuarioAsync("admin")).ReturnsAsync(existente);

			var (exito, error) = await _useCase.EditarAsync(1, "admin", true, "corta", new List<string>());

			Assert.False(exito);
			Assert.Equal("La nueva contraseña debe tener al menos 8 caracteres.", error);
		}
	}
}
