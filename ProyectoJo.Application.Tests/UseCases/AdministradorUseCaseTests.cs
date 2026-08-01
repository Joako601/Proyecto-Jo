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
			var (exito, error) = await _useCase.CrearAsync("nuevo", "1234567", new List<string>(), "clave123");

			Assert.False(exito);
			Assert.Equal("La contraseña debe tener al menos 8 caracteres.", error);
			_repository.Verify(r => r.AgregarAsync(It.IsAny<Administrador>()), Times.Never);
		}

		[Fact]
		public async Task CrearAsync_ConUsuarioYaExistente_DevuelveError()
		{
			_repository.Setup(r => r.ObtenerPorUsuarioAsync("admin2"))
				.ReturnsAsync(new Administrador { Id = 1, Usuario = "admin2" });

			var (exito, error) = await _useCase.CrearAsync("admin2", "contrasenaValida", new List<string>(), "clave123");

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

			var (exito, error) = await _useCase.CrearAsync(" nuevo ", "contrasenaValida", new List<string> { "Menu", "AreaInventada" }, "clave123");

			Assert.True(exito);
			Assert.Null(error);
			Assert.NotNull(guardado);
			Assert.Equal("nuevo", guardado!.Usuario);
			Assert.NotEqual("contrasenaValida", guardado.ContrasenaHash);
			Assert.Single(guardado.Areas);
			Assert.Equal("Menu", guardado.Areas[0]);
		}

		[Fact]
		public async Task CrearAsync_ConClaveSupervisorCorta_DevuelveErrorSinAgregar()
		{
			var (exito, error) = await _useCase.CrearAsync("nuevo", "contrasenaValida", new List<string>(), "1234");

			Assert.False(exito);
			Assert.Equal("La clave de supervisor debe tener al menos 6 caracteres.", error);
			_repository.Verify(r => r.AgregarAsync(It.IsAny<Administrador>()), Times.Never);
		}

		[Fact]
		public async Task CrearAsync_ConClaveSupervisorValida_LaHasheaYGuarda()
		{
			_repository.Setup(r => r.ObtenerPorUsuarioAsync("nuevo")).ReturnsAsync((Administrador?)null);

			Administrador? guardado = null;
			_repository.Setup(r => r.AgregarAsync(It.IsAny<Administrador>()))
				.Callback<Administrador>(a => guardado = a)
				.Returns(Task.CompletedTask);

			var (exito, error) = await _useCase.CrearAsync("nuevo", "contrasenaValida", new List<string>(), "clave123");

			Assert.True(exito);
			Assert.Null(error);
			Assert.NotNull(guardado!.ClaveSupervisorHash);
			Assert.NotEqual("clave123", guardado.ClaveSupervisorHash);
		}

		[Fact]
		public async Task CrearAsync_SinClaveSupervisor_DevuelveErrorSinAgregar()
		{
			var (exito, error) = await _useCase.CrearAsync("nuevo", "contrasenaValida", new List<string>(), null);

			Assert.False(exito);
			Assert.Equal("Usuario, contraseña y clave de supervisor son obligatorios.", error);
			_repository.Verify(r => r.AgregarAsync(It.IsAny<Administrador>()), Times.Never);
		}

		[Fact]
		public async Task EditarAsync_CuandoElAdministradorNoExiste_DevuelveError()
		{
			_repository.Setup(r => r.ObtenerPorIdAsync(999)).ReturnsAsync((Administrador?)null);

			var (exito, error) = await _useCase.EditarAsync(999, "usuario", true, null, new List<string>(), null);

			Assert.False(exito);
			Assert.Equal("El administrador no existe.", error);
		}

		[Fact]
		public async Task EditarAsync_SinNuevaContrasenaONuevaClaveSupervisor_DevuelveError()
		{
			var existente = new Administrador { Id = 1, Usuario = "admin", Areas = new List<string>() };
			_repository.Setup(r => r.ObtenerPorIdAsync(1)).ReturnsAsync(existente);

			var (exito, error) = await _useCase.EditarAsync(1, "admin", true, null, new List<string>(), null);

			Assert.False(exito);
			Assert.Equal("Usuario, contraseña y clave de supervisor son obligatorios.", error);
			_repository.Verify(r => r.ActualizarAsync(It.IsAny<Administrador>()), Times.Never);
		}

		[Fact]
		public async Task EditarAsync_ConNuevaContrasenaCorta_DevuelveError()
		{
			var existente = new Administrador { Id = 1, Usuario = "admin", Areas = new List<string>() };
			_repository.Setup(r => r.ObtenerPorIdAsync(1)).ReturnsAsync(existente);
			_repository.Setup(r => r.ObtenerPorUsuarioAsync("admin")).ReturnsAsync(existente);

			var (exito, error) = await _useCase.EditarAsync(1, "admin", true, "corta", new List<string>(), "claveNueva123");

			Assert.False(exito);
			Assert.Equal("La nueva contraseña debe tener al menos 8 caracteres.", error);
		}

		[Fact]
		public async Task EditarAsync_ConNuevaClaveSupervisorCorta_DevuelveError()
		{
			var existente = new Administrador { Id = 1, Usuario = "admin", Areas = new List<string>() };
			_repository.Setup(r => r.ObtenerPorIdAsync(1)).ReturnsAsync(existente);
			_repository.Setup(r => r.ObtenerPorUsuarioAsync("admin")).ReturnsAsync(existente);

			var (exito, error) = await _useCase.EditarAsync(1, "admin", true, "contrasenaValida", new List<string>(), "123");

			Assert.False(exito);
			Assert.Equal("La nueva clave de supervisor debe tener al menos 6 caracteres.", error);
		}

		[Fact]
		public async Task EditarAsync_ConDatosValidos_ActualizaContrasenaYClaveSupervisor()
		{
			var existente = new Administrador { Id = 1, Usuario = "admin", Areas = new List<string>(), ClaveSupervisorHash = "hash-viejo" };
			_repository.Setup(r => r.ObtenerPorIdAsync(1)).ReturnsAsync(existente);
			_repository.Setup(r => r.ObtenerPorUsuarioAsync("admin")).ReturnsAsync(existente);
			_repository.Setup(r => r.ActualizarAsync(existente)).ReturnsAsync(true);

			var (exito, error) = await _useCase.EditarAsync(1, "admin", true, "contrasenaValida", new List<string>(), "claveNueva123");

			Assert.True(exito);
			Assert.Null(error);
			Assert.NotEqual("hash-viejo", existente.ClaveSupervisorHash);
		}

		[Fact]
		public async Task EditarAsync_ConAccesoGeneral_GuardaAreaGeneral()
		{
			var existente = new Administrador { Id = 1, Usuario = "admin", Areas = new List<string>() };
			_repository.Setup(r => r.ObtenerPorIdAsync(1)).ReturnsAsync(existente);
			_repository.Setup(r => r.ObtenerPorUsuarioAsync("admin")).ReturnsAsync(existente);
			_repository.Setup(r => r.ActualizarAsync(existente)).ReturnsAsync(true);

			var (exito, _) = await _useCase.EditarAsync(1, "admin", true, "contrasenaValida", new List<string> { "General" }, "claveNueva123");

			Assert.True(exito);
			Assert.Single(existente.Areas);
			Assert.Equal("General", existente.Areas[0]);
		}

		[Fact]
		public async Task EditarAsync_SinAreasNiGeneral_DejaAreasVacias()
		{
			var existente = new Administrador { Id = 1, Usuario = "admin", Areas = new List<string> { "General" } };
			_repository.Setup(r => r.ObtenerPorIdAsync(1)).ReturnsAsync(existente);
			_repository.Setup(r => r.ObtenerPorUsuarioAsync("admin")).ReturnsAsync(existente);
			_repository.Setup(r => r.ActualizarAsync(existente)).ReturnsAsync(true);

			var (exito, _) = await _useCase.EditarAsync(1, "admin", true, "contrasenaValida", new List<string>(), "claveNueva123");

			Assert.True(exito);
			Assert.Empty(existente.Areas);
		}
	}
}
