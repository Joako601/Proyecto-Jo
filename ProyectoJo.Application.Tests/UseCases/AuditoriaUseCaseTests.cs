using Moq;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Application.UseCases;
using ProyectoJo.Domain.Entities;
using Xunit;

namespace ProyectoJo.Application.Tests.UseCases
{
	public class AuditoriaUseCaseTests
	{
		private readonly Mock<IAuditoriaRepository> _repository = new();
		private readonly AuditoriaUseCase _useCase;

		public AuditoriaUseCaseTests()
		{
			_useCase = new AuditoriaUseCase(_repository.Object);
		}

		[Fact]
		public void RegistrarAccion_GuardaUnRegistroConLosDatosProvistos()
		{
			RegistroAuditoria? guardado = null;
			_repository.Setup(r => r.Guardar(It.IsAny<RegistroAuditoria>()))
				.Callback<RegistroAuditoria>(r => guardado = r);

			_useCase.RegistrarAccion("admin", "Productos", TipoAccionAuditoria.Edicion, "Producto #1",
				detalleAntes: "antes", detalleDespues: "despues");

			Assert.NotNull(guardado);
			Assert.Equal("admin", guardado!.Usuario);
			Assert.Equal("Productos", guardado.Modulo);
			Assert.Equal(TipoAccionAuditoria.Edicion, guardado.Accion);
			Assert.Equal("Producto #1", guardado.Entidad);
			Assert.Equal("antes", guardado.DetalleAntes);
			Assert.Equal("despues", guardado.DetalleDespues);
		}

		[Fact]
		public void ObtenerHistorial_FiltraPorModuloSinImportarMayusculas()
		{
			var registros = new List<RegistroAuditoria>
			{
				new() { Modulo = "Productos", FechaHora = new DateTime(2026, 1, 1) },
				new() { Modulo = "finanzas", FechaHora = new DateTime(2026, 1, 2) }
			};
			_repository.Setup(r => r.ObtenerTodos()).Returns(registros);

			var resultado = _useCase.ObtenerHistorial(modulo: "PRODUCTOS");

			Assert.Single(resultado);
			Assert.Equal("Productos", resultado[0].Modulo);
		}

		[Fact]
		public void ObtenerHistorial_FiltraPorRangoDeFechasYOrdenaDescendente()
		{
			var registros = new List<RegistroAuditoria>
			{
				new() { Modulo = "A", FechaHora = new DateTime(2026, 1, 1) },
				new() { Modulo = "A", FechaHora = new DateTime(2026, 1, 5) },
				new() { Modulo = "A", FechaHora = new DateTime(2026, 1, 10) }
			};
			_repository.Setup(r => r.ObtenerTodos()).Returns(registros);

			var resultado = _useCase.ObtenerHistorial(desde: new DateTime(2026, 1, 2), hasta: new DateTime(2026, 1, 9));

			Assert.Single(resultado);
			Assert.Equal(new DateTime(2026, 1, 5), resultado[0].FechaHora);
		}
	}
}
