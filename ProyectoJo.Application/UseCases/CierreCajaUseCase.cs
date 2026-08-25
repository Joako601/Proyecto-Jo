using ProyectoJo.Application.Ports.In;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Application.UseCases
{
	public class CierreCajaUseCase : ICierreCajaService
	{
		private const string CategoriaVentas = "Ventas";

		private readonly ICierreCajaRepository _cierreCajaRepository;
		private readonly IFinanzaRepository _finanzaRepository;
		private readonly IAuditoriaService _auditoriaService;

		public CierreCajaUseCase(ICierreCajaRepository cierreCajaRepository, IFinanzaRepository finanzaRepository, IAuditoriaService auditoriaService)
		{
			_cierreCajaRepository = cierreCajaRepository;
			_finanzaRepository = finanzaRepository;
			_auditoriaService = auditoriaService;
		}

		public CierreCaja? ObtenerCajaAbierta() =>
			_cierreCajaRepository.ObtenerTodos()
				.FirstOrDefault(c => c.Estado == EstadoCaja.Abierta);

		public CierreCaja AbrirCaja(decimal fondoInicial, string? notas, string usuario)
		{
			var nuevaCaja = new CierreCaja
			{
				Estado = EstadoCaja.Abierta,
				FechaApertura = DateTime.Now,
				FondoInicial = fondoInicial,
				NotasApertura = notas
			};

			var abierta = _cierreCajaRepository.IntentarAbrir(nuevaCaja);
			if (!abierta)
				throw new InvalidOperationException("Ya hay una caja abierta. Cierra la caja actual antes de abrir una nueva.");

			_auditoriaService.RegistrarAccion(
				usuario: usuario,
				modulo: "CierreCaja",
				accion: TipoAccionAuditoria.Creacion,
				entidad: $"Caja #{nuevaCaja.Id}",
				detalleDespues: $"Apertura con fondo inicial ${fondoInicial}"
			);

			return nuevaCaja;
		}

		public CierreCaja CerrarCaja(int id, string? notas, string usuario)
		{
			var fechaCierre = DateTime.Now;
			decimal ventas = 0, gastos = 0;

			var (caja, error) = _cierreCajaRepository.CerrarAtomico(id, cajaBloqueada =>
			{
				if (cajaBloqueada.Estado == EstadoCaja.Cerrada)
					return "Esta caja ya fue cerrada.";

				(ventas, gastos) = CalcularMovimientosDelTurno(cajaBloqueada, fechaCierre);

				cajaBloqueada.VentasDelDia = ventas;
				cajaBloqueada.GastosDelDia = gastos;
				cajaBloqueada.NotasCierre = notas;
				cajaBloqueada.FechaCierre = fechaCierre;
				cajaBloqueada.Estado = EstadoCaja.Cerrada;
				return null;
			});

			if (caja is null)
				throw new InvalidOperationException(error ?? "No se pudo cerrar la caja.");

			_auditoriaService.RegistrarAccion(
				usuario: usuario,
				modulo: "CierreCaja",
				accion: TipoAccionAuditoria.Edicion,
				entidad: $"Caja #{caja.Id}",
				detalleDespues: $"Cierre - Ventas: ${ventas} - Gastos: ${gastos}"
			);

			return caja;
		}

		public CierreCaja ObtenerVistaPreviaCierre(int id)
		{
			var caja = ObtenerCajaParaCerrar(id);
			var (ventas, gastos) = CalcularMovimientosDelTurno(caja, DateTime.Now);

			// Snapshot calculado, no se persiste: es solo para mostrar antes de confirmar.
			return new CierreCaja
			{
				Id = caja.Id,
				Estado = caja.Estado,
				FechaApertura = caja.FechaApertura,
				FondoInicial = caja.FondoInicial,
				NotasApertura = caja.NotasApertura,
				VentasDelDia = ventas,
				GastosDelDia = gastos
			};
		}

		private CierreCaja ObtenerCajaParaCerrar(int id)
		{
			var caja = _cierreCajaRepository.ObtenerPorId(id)
				?? throw new InvalidOperationException("No se encontró la caja indicada.");

			if (caja.Estado == EstadoCaja.Cerrada)
				throw new InvalidOperationException("Esta caja ya fue cerrada.");

			return caja;
		}

		private (decimal Ventas, decimal Gastos) CalcularMovimientosDelTurno(CierreCaja caja, DateTime fechaReferencia)
		{
			var movimientosDelTurno = _finanzaRepository.ObtenerTodos()
				.Where(f => f.Fecha.Date >= caja.FechaApertura.Date && f.Fecha.Date <= fechaReferencia.Date)
				.ToList();

			var ventas = movimientosDelTurno
				.Where(f => f.Tipo == TipoMovimiento.Ingreso &&
							string.Equals(f.Categoria?.Trim(), CategoriaVentas, StringComparison.OrdinalIgnoreCase))
				.Sum(f => f.Monto);

			var gastos = movimientosDelTurno
				.Where(f => f.Tipo == TipoMovimiento.Egreso)
				.Sum(f => f.Monto);

			return (ventas, gastos);
		}

		public List<CierreCaja> ObtenerHistorial() =>
			_cierreCajaRepository.ObtenerTodos()
				.OrderByDescending(c => c.FechaApertura)
				.ToList();

		public CierreCaja? ObtenerPorId(int id) => _cierreCajaRepository.ObtenerPorId(id);
	}
}