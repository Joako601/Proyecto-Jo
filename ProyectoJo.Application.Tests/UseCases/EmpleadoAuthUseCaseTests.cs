using Moq;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Application.UseCases;
using ProyectoJo.Domain.Entities;
using Xunit;

namespace ProyectoJo.Application.Tests.UseCases
{
	public class EmpleadoAuthUseCaseTests
	{
		private readonly Mock<IEmpleadoRepository> _repository = new();
		private readonly EmpleadoAuthUseCase _useCase;

		public EmpleadoAuthUseCaseTests()
		{
			_useCase = new EmpleadoAuthUseCase(_repository.Object);
		}

		[Fact]
		public async Task ValidarCredencialesAsync_ConNombreVacio_DevuelveNullSinConsultarRepositorio()
		{
			var resultado = await _useCase.ValidarCredencialesAsync("", "clave123", RolEmpleado.Cocina);

			Assert.Null(resultado);
			_repository.Verify(r => r.ObtenerTodosAsync(), Times.Never);
		}

		[Fact]
		public async Task ValidarCredencialesAsync_ConEmpleadoInactivo_DevuelveNull()
		{
			var empleado = new Empleado
			{
				Id = 1,
				Nombre = "Juan",
				Rol = RolEmpleado.Cocina,
				Activo = false,
				ClaveHash = EmpleadoAuthUseCase.HashearClave("clave123")
			};
			_repository.Setup(r => r.ObtenerTodosAsync()).ReturnsAsync(new List<Empleado> { empleado });

			var resultado = await _useCase.ValidarCredencialesAsync("Juan", "clave123", RolEmpleado.Cocina);

			Assert.Null(resultado);
		}

		[Fact]
		public async Task ValidarCredencialesAsync_ConRolDistintoAlDeLaEstacion_DevuelveNull()
		{
			var empleado = new Empleado
			{
				Id = 1,
				Nombre = "Juan",
				Rol = RolEmpleado.Cocina,
				Activo = true,
				ClaveHash = EmpleadoAuthUseCase.HashearClave("clave123")
			};
			_repository.Setup(r => r.ObtenerTodosAsync()).ReturnsAsync(new List<Empleado> { empleado });

			var resultado = await _useCase.ValidarCredencialesAsync("Juan", "clave123", RolEmpleado.Recepcion);

			Assert.Null(resultado);
		}

		[Fact]
		public async Task ValidarCredencialesAsync_ConClaveIncorrecta_DevuelveNull()
		{
			var empleado = new Empleado
			{
				Id = 1,
				Nombre = "Juan",
				Rol = RolEmpleado.Cocina,
				Activo = true,
				ClaveHash = EmpleadoAuthUseCase.HashearClave("clave123")
			};
			_repository.Setup(r => r.ObtenerTodosAsync()).ReturnsAsync(new List<Empleado> { empleado });

			var resultado = await _useCase.ValidarCredencialesAsync("Juan", "claveIncorrecta", RolEmpleado.Cocina);

			Assert.Null(resultado);
		}

		[Fact]
		public async Task ValidarCredencialesAsync_ConCredencialesCorrectas_DevuelveElEmpleadoIgnorandoMayusculasYEspacios()
		{
			var empleado = new Empleado
			{
				Id = 1,
				Nombre = "Juan",
				Rol = RolEmpleado.Cocina,
				Activo = true,
				ClaveHash = EmpleadoAuthUseCase.HashearClave("clave123")
			};
			_repository.Setup(r => r.ObtenerTodosAsync()).ReturnsAsync(new List<Empleado> { empleado });

			var resultado = await _useCase.ValidarCredencialesAsync(" juan ", "clave123", RolEmpleado.Cocina);

			Assert.Same(empleado, resultado);
		}
	}
}
