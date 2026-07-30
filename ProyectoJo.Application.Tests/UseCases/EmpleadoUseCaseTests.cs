using Moq;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Application.UseCases;
using ProyectoJo.Domain.Entities;
using Xunit;

namespace ProyectoJo.Application.Tests.UseCases
{
	public class EmpleadoUseCaseTests
	{
		private readonly Mock<IEmpleadoRepository> _repository = new();
		private readonly EmpleadoUseCase _useCase;

		public EmpleadoUseCaseTests()
		{
			_useCase = new EmpleadoUseCase(_repository.Object);
		}

		[Fact]
		public async Task CrearAsync_ConClaveCorta_DevuelveErrorSinAgregar()
		{
			_repository.Setup(r => r.ObtenerTodosAsync()).ReturnsAsync(new List<Empleado>());

			var (exito, error) = await _useCase.CrearAsync("Juan", "123", RolEmpleado.Cocina);

			Assert.False(exito);
			Assert.Contains("al menos", error);
			_repository.Verify(r => r.AgregarAsync(It.IsAny<Empleado>()), Times.Never);
		}

		[Fact]
		public async Task CrearAsync_ConNombreDuplicadoEnLaMismaEstacion_DevuelveError()
		{
			var existentes = new List<Empleado>
			{
				new() { Id = 1, Nombre = "Juan", Rol = RolEmpleado.Cocina }
			};
			_repository.Setup(r => r.ObtenerTodosAsync()).ReturnsAsync(existentes);

			var (exito, error) = await _useCase.CrearAsync("juan", "clave123", RolEmpleado.Cocina);

			Assert.False(exito);
			Assert.Equal("Ya existe un operador con ese nombre en esa estación.", error);
			_repository.Verify(r => r.AgregarAsync(It.IsAny<Empleado>()), Times.Never);
		}

		[Fact]
		public async Task CrearAsync_ConMismoNombreEnOtraEstacion_NoLoConsideraDuplicado()
		{
			var existentes = new List<Empleado>
			{
				new() { Id = 1, Nombre = "Juan", Rol = RolEmpleado.Recepcion }
			};
			_repository.Setup(r => r.ObtenerTodosAsync()).ReturnsAsync(existentes);

			var (exito, error) = await _useCase.CrearAsync("Juan", "clave123", RolEmpleado.Cocina);

			Assert.True(exito);
			Assert.Null(error);
			_repository.Verify(r => r.AgregarAsync(It.Is<Empleado>(e => e.Nombre == "Juan" && e.Rol == RolEmpleado.Cocina)), Times.Once);
		}

		[Fact]
		public async Task EditarAsync_CuandoElEmpleadoNoExiste_DevuelveError()
		{
			_repository.Setup(r => r.ObtenerPorIdAsync(999)).ReturnsAsync((Empleado?)null);

			var (exito, error) = await _useCase.EditarAsync(999, "Juan", true, RolEmpleado.Cocina, null);

			Assert.False(exito);
			Assert.Equal("El operador no existe.", error);
		}

		[Fact]
		public async Task EditarAsync_ConNuevaClaveCorta_DevuelveErrorSinActualizar()
		{
			var empleado = new Empleado { Id = 1, Nombre = "Juan", Rol = RolEmpleado.Cocina };
			_repository.Setup(r => r.ObtenerPorIdAsync(1)).ReturnsAsync(empleado);
			_repository.Setup(r => r.ObtenerTodosAsync()).ReturnsAsync(new List<Empleado> { empleado });

			var (exito, error) = await _useCase.EditarAsync(1, "Juan", true, RolEmpleado.Cocina, "123");

			Assert.False(exito);
			Assert.Contains("al menos", error);
			_repository.Verify(r => r.ActualizarAsync(It.IsAny<Empleado>()), Times.Never);
		}
	}
}
