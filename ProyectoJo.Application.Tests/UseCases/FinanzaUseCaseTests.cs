using System.Timers;
using Moq;
using ProyectoJo.Application.Ports.In;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Application.UseCases;
using ProyectoJo.Domain.Entities;
using Xunit;

namespace ProyectoJo.Application.Tests.UseCases
{
	public class FinanzaUseCaseTests
	{
		private readonly Mock<IFinanzaRepository> _repository = new();
		private readonly Mock<IAuditoriaService> _auditoriaService = new();
		private readonly FinanzaUseCase _useCase;

		public FinanzaUseCaseTests()
		{
			_useCase = new FinanzaUseCase(_repository.Object, _auditoriaService.Object);
		}

		[Fact]
		public void Editar_CuandoLaFinanzaNoExiste_DevuelveFalseYNoRegistraAuditoria()
		{
			// Arrange
			_repository.Setup(r => r.ObtenerPorId(999)).Returns((Finanza?)null);
			var finanzaInexistente = new Finanza { Id = 999, Monto = 100, Categoria = "Ventas", Descripcion = "x", Tipo = TipoMovimiento.Ingreso };

			// Act
			var resultado = _useCase.Editar(finanzaInexistente, "admin");

			// Assert
			Assert.False(resultado);
			_repository.Verify(r => r.Actualizar(It.IsAny<Finanza>()), Times.Never);
			_auditoriaService.Verify(a => a.RegistrarAccion(
				It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TipoAccionAuditoria>(), It.IsAny<string>(),
				It.IsAny<string?>(), It.IsAny<string?>()), Times.Never);
		}

		[Fact]
		public void Editar_CuandoLaFinanzaExiste_DevuelveTrueYRegistraAuditoria()
		{
			// Arrange
			var anterior = new Finanza { Id = 1, Monto = 100, Categoria = "Ventas", Descripcion = "x", Tipo = TipoMovimiento.Ingreso };
			_repository.Setup(r => r.ObtenerPorId(1)).Returns(anterior);
			var finanzaEditada = new Finanza { Id = 1, Monto = 150, Categoria = "Ventas", Descripcion = "y", Tipo = TipoMovimiento.Ingreso };

			// Act
			var resultado = _useCase.Editar(finanzaEditada, "admin");

			// Assert
			Assert.True(resultado);
			_repository.Verify(r => r.Actualizar(finanzaEditada), Times.Once);
			_auditoriaService.Verify(a => a.RegistrarAccion(
				"admin", "Finanzas", TipoAccionAuditoria.Edicion, It.IsAny<string>(),
				It.IsAny<string?>(), It.IsAny<string?>()), Times.Once);
		}

		[Fact]
		public void Eliminar_CuandoLaFinanzaNoExiste_DevuelveFalseYNoRegistraAuditoria()
		{
			// Arrange
			_repository.Setup(r => r.ObtenerPorId(999)).Returns((Finanza?)null);

			// Act
			var resultado = _useCase.Eliminar(999, "admin");

			// Assert
			Assert.False(resultado);
			_repository.Verify(r => r.Eliminar(It.IsAny<int>()), Times.Never);
			_auditoriaService.Verify(a => a.RegistrarAccion(
				It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TipoAccionAuditoria>(), It.IsAny<string>(),
				It.IsAny<string?>(), It.IsAny<string?>()), Times.Never);
		}

		[Fact]
		public void Eliminar_CuandoLaFinanzaExiste_DevuelveTrueYRegistraAuditoria()
		{
			// Arrange
			var finanza = new Finanza { Id = 1, Monto = 100, Categoria = "Ventas", Descripcion = "x", Tipo = TipoMovimiento.Ingreso };
			_repository.Setup(r => r.ObtenerPorId(1)).Returns(finanza);

			// Act
			var resultado = _useCase.Eliminar(1, "admin");

			// Assert
			Assert.True(resultado);
			_repository.Verify(r => r.Eliminar(1), Times.Once);
			_auditoriaService.Verify(a => a.RegistrarAccion(
				"admin", "Finanzas", TipoAccionAuditoria.Eliminacion, It.IsAny<string>(),
				It.IsAny<string?>(), It.IsAny<string?>()), Times.Once);
		}

		[Fact]
		public void RegistrarMovimiento_ConFechaPorDefecto_AsignaLaFechaActual()
		{
			// Arrange
			var finanza = new Finanza { Monto = 100, Tipo = TipoMovimiento.Ingreso, Categoria = "Ventas", Descripcion = "x" };

			// Act
			_useCase.RegistrarMovimiento(finanza, "admin");

			// Assert
			Assert.True((DateTime.Now - finanza.Fecha) < TimeSpan.FromSeconds(5));
			_repository.Verify(r => r.Guardar(finanza), Times.Once);
			_auditoriaService.Verify(a => a.RegistrarAccion(
				"admin", "Finanzas", TipoAccionAuditoria.Creacion, It.IsAny<string>(),
				It.IsAny<string?>(), It.IsAny<string?>()), Times.Once);
		}

		[Fact]
		public void RegistrarMovimiento_ConFechaExplicita_ConservaLaFechaProvista()
		{
			// Arrange
			var fecha = new DateTime(2026, 1, 15);
			var finanza = new Finanza { Monto = 100, Tipo = TipoMovimiento.Egreso, Categoria = "Insumos", Descripcion = "x", Fecha = fecha };

			// Act
			_useCase.RegistrarMovimiento(finanza, "admin");

			// Assert
			Assert.Equal(fecha, finanza.Fecha);
		}

		[Fact]
		public void ObtenerDashboard_VentasDia_SoloSumaIngresosRegistradosHoy()
		{
			// Arrange
			var hoy = DateTime.Today;
			_repository.Setup(r => r.ObtenerTodos()).Returns(new List<Finanza>
			{
				new() { Tipo = TipoMovimiento.Ingreso, Categoria = "Ventas", Monto = 100, Fecha = hoy },
				new() { Tipo = TipoMovimiento.Ingreso, Categoria = "Ventas", Monto = 50, Fecha = hoy.AddDays(-1) },
				new() { Tipo = TipoMovimiento.Egreso, Categoria = "Insumos", Monto = 20, Fecha = hoy }
			});

			// Act
			var dashboard = _useCase.ObtenerDashboard();

			// Assert
			Assert.Equal(100m, dashboard.VentasDia);
		}

		[Fact]
		public void ObtenerDashboard_VentasMes_SumaTodosLosIngresosDelMesActualSinImportarElDia()
		{
			// Arrange
			var hoy = DateTime.Today;
			var otroDiaDelMes = hoy.Day > 1 ? hoy.AddDays(-1) : hoy.AddDays(1);
			_repository.Setup(r => r.ObtenerTodos()).Returns(new List<Finanza>
			{
				new() { Tipo = TipoMovimiento.Ingreso, Categoria = "Ventas", Monto = 100, Fecha = hoy },
				new() { Tipo = TipoMovimiento.Ingreso, Categoria = "Ventas", Monto = 50, Fecha = otroDiaDelMes },
				new() { Tipo = TipoMovimiento.Ingreso, Categoria = "Ventas", Monto = 999, Fecha = hoy.AddMonths(-1) }
			});

			// Act
			var dashboard = _useCase.ObtenerDashboard();

			// Assert
			Assert.Equal(150m, dashboard.VentasMes);
		}

		[Fact]
		public void ObtenerDashboard_VentasAnio_SumaIngresosDeCualquierMesDelAnioActual()
		{
			// Arrange
			var hoy = DateTime.Today;
			var otroMes = hoy.Month == 1 ? 2 : 1;
			var fechaOtroMes = new DateTime(hoy.Year, otroMes, 1);
			_repository.Setup(r => r.ObtenerTodos()).Returns(new List<Finanza>
			{
				new() { Tipo = TipoMovimiento.Ingreso, Categoria = "Ventas", Monto = 100, Fecha = hoy },
				new() { Tipo = TipoMovimiento.Ingreso, Categoria = "Ventas", Monto = 60, Fecha = fechaOtroMes },
				new() { Tipo = TipoMovimiento.Ingreso, Categoria = "Ventas", Monto = 999, Fecha = hoy.AddYears(-1) }
			});

			// Act
			var dashboard = _useCase.ObtenerDashboard();

			// Assert
			Assert.Equal(160m, dashboard.VentasAnio);
		}

		[Fact]
		public void ObtenerDashboard_TicketPromedioMesPasado_DivideLasVentasEntreLaCantidadDeMovimientos()
		{
			// Arrange
			var mesPasado = DateTime.Today.AddMonths(-1);
			_repository.Setup(r => r.ObtenerTodos()).Returns(new List<Finanza>
			{
				new() { Tipo = TipoMovimiento.Ingreso, Categoria = "Ventas", Monto = 100, Fecha = mesPasado },
				new() { Tipo = TipoMovimiento.Ingreso, Categoria = "Ventas", Monto = 50, Fecha = mesPasado }
			});

			// Act
			var dashboard = _useCase.ObtenerDashboard();

			// Assert
			Assert.Equal(150m, dashboard.VentasMesPasado);
			Assert.Equal(75m, dashboard.TicketPromedioMesPasado);
		}

		[Fact]
		public void ObtenerDashboard_SinVentasElMesPasado_TicketPromedioEsCeroSinDividirPorCero()
		{
			// Arrange
			_repository.Setup(r => r.ObtenerTodos()).Returns(new List<Finanza>());

			// Act
			var dashboard = _useCase.ObtenerDashboard();

			// Assert
			Assert.Equal(0m, dashboard.TicketPromedioMesPasado);
		}

		[Fact]
		public void ObtenerDashboard_TopCategorias_OrdenaPorTotalDescendenteYLimitaACinco()
		{
			// Arrange: 6 categorías de egresos, la más chica (Cat5) debe quedar fuera del top 5
			var hoy = DateTime.Today;
			var montos = new[] { 100, 200, 300, 400, 500, 50 };
			var finanzas = montos
				.Select((monto, i) => new Finanza { Tipo = TipoMovimiento.Egreso, Categoria = $"Cat{i}", Monto = monto, Fecha = hoy })
				.ToList();
			_repository.Setup(r => r.ObtenerTodos()).Returns(finanzas);

			// Act
			var dashboard = _useCase.ObtenerDashboard();

			// Assert
			Assert.Equal(5, dashboard.TopCategorias.Count);
			Assert.DoesNotContain(dashboard.TopCategorias, c => c.Categoria == "Cat5");
			Assert.Equal("Cat4", dashboard.TopCategorias[0].Categoria);
		}

		[Fact]
		public void ObtenerDashboard_TendenciaAnio_TieneLosDoceMesesDelAnioActualEnOrden()
		{
			// Arrange
			_repository.Setup(r => r.ObtenerTodos()).Returns(new List<Finanza>());

			// Act
			var dashboard = _useCase.ObtenerDashboard();

			// Assert
			Assert.Equal(12, dashboard.TendenciaAnio.Count);
			Assert.Equal(Enumerable.Range(1, 12), dashboard.TendenciaAnio.Select(t => t.Mes));
			Assert.All(dashboard.TendenciaAnio, t => Assert.Equal(DateTime.Today.Year, t.Anio));
		}

		[Fact]
		public void ObtenerDashboard_UltimosSeisMeses_IncluyeElMesActualComoElMasReciente()
		{
			// Arrange
			_repository.Setup(r => r.ObtenerTodos()).Returns(new List<Finanza>());

			// Act
			var dashboard = _useCase.ObtenerDashboard();

			// Assert
			Assert.Equal(6, dashboard.UltimosSeisMeses.Count);
			var masReciente = dashboard.UltimosSeisMeses.Last();
			Assert.Equal(DateTime.Today.Month, masReciente.Mes);
			Assert.Equal(DateTime.Today.Year, masReciente.Anio);
		}
	}
}