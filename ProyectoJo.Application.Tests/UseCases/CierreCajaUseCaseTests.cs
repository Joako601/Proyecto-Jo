using Moq;
using ProyectoJo.Application.Ports.In;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Application.UseCases;
using ProyectoJo.Domain.Entities;
using Xunit;

namespace ProyectoJo.Application.Tests.UseCases
{
	public class CierreCajaUseCaseTests
	{
		private readonly Mock<ICierreCajaRepository> _cierreCajaRepository = new();
		private readonly Mock<IFinanzaRepository> _finanzaRepository = new();
		private readonly Mock<IAuditoriaService> _auditoriaService = new();
		private readonly CierreCajaUseCase _useCase;

		public CierreCajaUseCaseTests()
		{
			_useCase = new CierreCajaUseCase(_cierreCajaRepository.Object, _finanzaRepository.Object, _auditoriaService.Object);
		}

		[Fact]
		public void AbrirCaja_CuandoYaHayUnaCajaAbierta_LanzaExcepcionYNoRegistraAuditoria()
		{
			// Arrange: el repositorio simula que ya existe una caja abierta,
			_cierreCajaRepository.Setup(r => r.IntentarAbrir(It.IsAny<CierreCaja>())).Returns(false);

			// Act + Assert
			var excepcion = Assert.Throws<InvalidOperationException>(
				() => _useCase.AbrirCaja(fondoInicial: 500, notas: null, usuario: "admin"));

			Assert.Contains("Ya hay una caja abierta", excepcion.Message);
			_auditoriaService.Verify(a => a.RegistrarAccion(
				It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TipoAccionAuditoria>(), It.IsAny<string>(),
				It.IsAny<string?>(), It.IsAny<string?>()), Times.Never);
		}

		[Fact]
		public void AbrirCaja_CuandoNoHayCajaAbierta_DevuelveLaCajaYRegistraAuditoria()
		{
			// Arrange: el repositorio simula que pudo abrir sin problema
			_cierreCajaRepository
				.Setup(r => r.IntentarAbrir(It.IsAny<CierreCaja>()))
				.Callback<CierreCaja>(c => c.Id = 1)
				.Returns(true);

			// Act
			var resultado = _useCase.AbrirCaja(fondoInicial: 500, notas: "Apertura turno mañana", usuario: "admin");

			// Assert
			Assert.Equal(EstadoCaja.Abierta, resultado.Estado);
			Assert.Equal(500, resultado.FondoInicial);
			_cierreCajaRepository.Verify(r => r.IntentarAbrir(It.IsAny<CierreCaja>()), Times.Once);
			_auditoriaService.Verify(a => a.RegistrarAccion(
				"admin", "CierreCaja", TipoAccionAuditoria.Creacion, It.IsAny<string>(),
				It.IsAny<string?>(), It.IsAny<string?>()), Times.Once);
		}

		private void ConfigurarCerrarAtomico(CierreCaja caja)
		{
			_cierreCajaRepository
				.Setup(r => r.CerrarAtomico(caja.Id, It.IsAny<Func<CierreCaja, string?>>()))
				.Returns<int, Func<CierreCaja, string?>>((id, aplicarCierre) =>
				{
					var error = aplicarCierre(caja);
					return error is not null ? ((CierreCaja?)null, error) : (caja, (string?)null);
				});
		}

		[Fact]
		public void CerrarCaja_CuandoLaCajaNoExiste_LanzaExcepcion()
		{
			// Arrange: simula que CerrarAtomico no encontró la fila bajo lock
			_cierreCajaRepository
				.Setup(r => r.CerrarAtomico(999, It.IsAny<Func<CierreCaja, string?>>()))
				.Returns(((CierreCaja?)null, "No se encontró la caja indicada."));

			// Act & Assert
			var excepcion = Assert.Throws<InvalidOperationException>(
				() => _useCase.CerrarCaja(999, notas: null, usuario: "admin"));
			Assert.Contains("No se encontró la caja", excepcion.Message);
		}

		[Fact]
		public void CerrarCaja_CuandoLaCajaYaEstaCerrada_LanzaExcepcion()
		{
			// Arrange
			var cajaCerrada = new CierreCaja { Id = 1, Estado = EstadoCaja.Cerrada };
			ConfigurarCerrarAtomico(cajaCerrada);

			// Act & Assert
			var excepcion = Assert.Throws<InvalidOperationException>(
				() => _useCase.CerrarCaja(1, notas: null, usuario: "admin"));
			Assert.Contains("ya fue cerrada", excepcion.Message);
		}

		[Fact]
		public void CerrarCaja_SoloSumaIngresosDeCategoriaVentasYTodosLosEgresosDelTurno()
		{
			// Arrange
			var apertura = DateTime.Today.AddDays(-1);
			var caja = new CierreCaja { Id = 1, Estado = EstadoCaja.Abierta, FechaApertura = apertura, FondoInicial = 1000 };
			ConfigurarCerrarAtomico(caja);
			_finanzaRepository.Setup(r => r.ObtenerTodos()).Returns(new List<Finanza>
			{
				new() { Tipo = TipoMovimiento.Ingreso, Categoria = "Ventas", Monto = 500, Fecha = apertura },
				new() { Tipo = TipoMovimiento.Ingreso, Categoria = "Propinas", Monto = 80, Fecha = apertura }, // no cuenta como venta
				new() { Tipo = TipoMovimiento.Egreso, Categoria = "Insumos", Monto = 120, Fecha = apertura },
				new() { Tipo = TipoMovimiento.Ingreso, Categoria = "Ventas", Monto = 300, Fecha = apertura.AddDays(-5) } // antes de la apertura, no cuenta
			});

			// Act
			var resultado = _useCase.CerrarCaja(1, notas: null, usuario: "admin");

			// Assert
			Assert.Equal(500m, resultado.VentasDelDia);
			Assert.Equal(120m, resultado.GastosDelDia);
		}

		[Fact]
		public void CerrarCaja_ConCategoriaVentasSinImportarMayusculasNiEspacios_LaCuentaComoVenta()
		{
			// Arrange
			var apertura = DateTime.Today;
			var caja = new CierreCaja { Id = 1, Estado = EstadoCaja.Abierta, FechaApertura = apertura };
			ConfigurarCerrarAtomico(caja);
			_finanzaRepository.Setup(r => r.ObtenerTodos()).Returns(new List<Finanza>
			{
				new() { Tipo = TipoMovimiento.Ingreso, Categoria = " ventas ", Monto = 200, Fecha = apertura }
			});

			// Act
			var resultado = _useCase.CerrarCaja(1, notas: null, usuario: "admin");

			// Assert
			Assert.Equal(200m, resultado.VentasDelDia);
		}

		[Fact]
		public void CerrarCaja_ActualizaLaCajaYRegistraAuditoriaConLosTotales()
		{
			// Arrange
			var apertura = DateTime.Today;
			var caja = new CierreCaja { Id = 1, Estado = EstadoCaja.Abierta, FechaApertura = apertura, FondoInicial = 500 };
			ConfigurarCerrarAtomico(caja);
			_finanzaRepository.Setup(r => r.ObtenerTodos()).Returns(new List<Finanza>());

			// Act
			var resultado = _useCase.CerrarCaja(1, notas: "Turno tranquilo", usuario: "admin");

			// Assert
			Assert.Equal(EstadoCaja.Cerrada, resultado.Estado);
			Assert.Equal("Turno tranquilo", resultado.NotasCierre);
			Assert.NotNull(resultado.FechaCierre);
			_cierreCajaRepository.Verify(r => r.CerrarAtomico(1, It.IsAny<Func<CierreCaja, string?>>()), Times.Once);
			_auditoriaService.Verify(a => a.RegistrarAccion(
				"admin", "CierreCaja", TipoAccionAuditoria.Edicion, It.IsAny<string>(),
				It.IsAny<string?>(), It.IsAny<string?>()), Times.Once);
		}

		[Fact]
		public void ObtenerVistaPreviaCierre_CalculaLosTotalesSinPersistirCambios()
		{
			// Arrange
			var apertura = DateTime.Today;
			var caja = new CierreCaja { Id = 1, Estado = EstadoCaja.Abierta, FechaApertura = apertura, FondoInicial = 500 };
			_cierreCajaRepository.Setup(r => r.ObtenerPorId(1)).Returns(caja);
			_finanzaRepository.Setup(r => r.ObtenerTodos()).Returns(new List<Finanza>
			{
				new() { Tipo = TipoMovimiento.Ingreso, Categoria = "Ventas", Monto = 300, Fecha = apertura }
			});

			// Act
			var vistaPrevia = _useCase.ObtenerVistaPreviaCierre(1);

			// Assert: es un snapshot, no debe tocar el repositorio ni la auditoría
			Assert.Equal(300m, vistaPrevia.VentasDelDia);
			Assert.Equal(EstadoCaja.Abierta, vistaPrevia.Estado);
			_cierreCajaRepository.Verify(r => r.Actualizar(It.IsAny<CierreCaja>()), Times.Never);
			_auditoriaService.Verify(a => a.RegistrarAccion(
				It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TipoAccionAuditoria>(), It.IsAny<string>(),
				It.IsAny<string?>(), It.IsAny<string?>()), Times.Never);
		}
	}
}