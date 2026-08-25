using Moq;
using ProyectoJo.Application.Ports.In;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Application.UseCases;
using ProyectoJo.Domain.Entities;
using Xunit;

namespace ProyectoJo.Application.Tests.UseCases
{
	public class OpinionUseCaseTests
	{
		private readonly Mock<IOpinionRepository> _repository = new();
		private readonly Mock<IProductoService> _productoService = new();
		private readonly Mock<IAuditoriaService> _auditoriaService = new();
		private readonly OpinionUseCase _useCase;

		public OpinionUseCaseTests()
		{
			_useCase = new OpinionUseCase(_repository.Object, _productoService.Object, _auditoriaService.Object);
		}

		[Fact]
		public void ObtenerTodas_ArmaDtoConElPlatilloDelProductoAsociado()
		{
			var opinion = new OpinionCliente { Id = 1, ItemId = 5, Comentario = "Rico", Calificacion = 5, Fecha = DateTime.Now };
			_repository.Setup(r => r.ObtenerTodas()).Returns(new List<OpinionCliente> { opinion });
			_productoService.Setup(p => p.ObtenerTodos()).Returns(new List<Item> { new() { Id = 5, Platillo = "Tacos" } });

			var resultado = _useCase.ObtenerTodas();

			Assert.Single(resultado);
			Assert.Equal("Tacos", resultado[0].Platillo);
		}

		[Fact]
		public void ObtenerTodas_CuandoElProductoFueEliminado_MarcaPlatilloEliminado()
		{
			var opinion = new OpinionCliente { Id = 1, ItemId = 99, Comentario = "Rico", Calificacion = 4 };
			_repository.Setup(r => r.ObtenerTodas()).Returns(new List<OpinionCliente> { opinion });
			_productoService.Setup(p => p.ObtenerTodos()).Returns(new List<Item>());

			var resultado = _useCase.ObtenerTodas();

			Assert.Equal("Platillo eliminado", resultado[0].Platillo);
		}

		[Fact]
		public void Agregar_AsignaFechaYRegistradoPorYRegistraAuditoria()
		{
			var opinion = new OpinionCliente { ItemId = null, Comentario = "General", Calificacion = 5, Estado = EstadoSemaforo.Verde };

			_useCase.Agregar(opinion, "recepcionista1");

			Assert.Equal("recepcionista1", opinion.RegistradoPor);
			Assert.True((DateTime.Now - opinion.Fecha) < TimeSpan.FromSeconds(5));
			_repository.Verify(r => r.Agregar(opinion), Times.Once);
			_auditoriaService.Verify(a => a.RegistrarAccion(
				"recepcionista1", "Semáforo Feedback", TipoAccionAuditoria.Creacion, It.IsAny<string>(),
				It.IsAny<string?>(), It.IsAny<string?>()), Times.Once);
		}

		[Fact]
		public void Editar_CuandoLaOpinionNoExiste_DevuelveFalse()
		{
			_repository.Setup(r => r.ObtenerPorId(999)).Returns((OpinionCliente?)null);

			var resultado = _useCase.Editar(new OpinionCliente { Id = 999 }, "admin");

			Assert.False(resultado);
			_repository.Verify(r => r.Editar(It.IsAny<OpinionCliente>()), Times.Never);
		}

		[Fact]
		public void Editar_CuandoLaOpinionExiste_PreservaFechaYRegistradoPorOriginales()
		{
			var fechaOriginal = new DateTime(2026, 1, 1);
			var anterior = new OpinionCliente { Id = 1, Fecha = fechaOriginal, RegistradoPor = "recepcionista1", Calificacion = 3, Estado = EstadoSemaforo.Amarillo };
			_repository.Setup(r => r.ObtenerPorId(1)).Returns(anterior);

			var editada = new OpinionCliente { Id = 1, Calificacion = 5, Estado = EstadoSemaforo.Verde };
			_repository.Setup(r => r.Editar(editada)).Returns(true);

			var resultado = _useCase.Editar(editada, "admin");

			Assert.True(resultado);
			Assert.Equal(fechaOriginal, editada.Fecha);
			Assert.Equal("recepcionista1", editada.RegistradoPor);
		}
	}
}
