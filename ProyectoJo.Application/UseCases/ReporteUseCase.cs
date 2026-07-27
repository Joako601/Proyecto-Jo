using ProyectoJo.Application.Ports.In;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Domain.Entities;
using ProyectoJo.Application.DTOs;
using System.Runtime.InteropServices;

namespace ProyectoJo.Application.UseCases
{
	public class ReporteUseCase : IReporteService
	{
		private readonly IPedidoRepository _pedidoRepository;

		public ReporteUseCase(IPedidoRepository pedidoRepository)
		{
			_pedidoRepository = pedidoRepository;
		}

		public async Task<ResumenMapaCalor> ObtenerMapaCalorAsync(
			DateTime? desde = null,
			DateTime? hasta = null,
			bool semanaHistoricoCompleto = true,
			int semanaOffset = 0,
			int? anioMeses = null,
			int? mesDetalle = null)
		{
			var todos = await _pedidoRepository.ObtenerTodosAsync();

			var hoy = DateTime.UtcNow.Date;
			var fechaSeleccionada = (desde ?? hoy).Date;
			var anioMesesSeleccionado = anioMeses ?? hoy.Year;

			int diffHastaLunes = ((int)hoy.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
			var lunesSemanaActual = hoy.AddDays(-diffHastaLunes);
			var inicioSemana = lunesSemanaActual.AddDays(7 * semanaOffset);
			var finSemana = inicioSemana.AddDays(6);


			var horaCounts = new (int cant, decimal total)[24];
			var diaSemanaCounts = new (int cant, decimal total)[7];
			var porDiaDict = new Dictionary<DateTime, (int cant, decimal total)>();
			var porMesCounts = new (int cant, decimal total)[13]; // índice 1-12
			var porDiaMesDict = new Dictionary<DateTime, (int cant, decimal total)>();
			var topProductosDict = new Dictionary<string, (int cant, decimal total)>();

			int totalPedidosDelDia = 0;
			decimal totalVendidoDelDia = 0;

			foreach (var p in todos)
			{
				if (p.Estado != EstadoPedido.Pagado) continue;

				var fecha = p.FechaCreacion.Date;

				// Ventas por hora (solo día seleccionado)
				if (fecha == fechaSeleccionada)
				{
					ref var h = ref horaCounts[p.FechaCreacion.Hour];
					h.cant++; h.total += p.Total;
					totalPedidosDelDia++;
					totalVendidoDelDia += p.Total;
				}

				// Ventas por día de semana (histórico completo o solo semana actual)
				bool incluirEnDiaSemana = semanaHistoricoCompleto || (fecha >= inicioSemana && fecha <= finSemana);
				if (incluirEnDiaSemana)
				{
					ref var d = ref diaSemanaCounts[(int)p.FechaCreacion.DayOfWeek];
					d.cant++; d.total += p.Total;
				}

				// Historial día por día (todo el histórico)
				ref var porDia = ref CollectionsMarshal.GetValueRefOrAddDefault(porDiaDict, fecha, out _);
				porDia.cant++; porDia.total += p.Total;

				// Ventas por mes (solo año seleccionado)
				if (p.FechaCreacion.Year == anioMesesSeleccionado)
				{
					ref var m = ref porMesCounts[p.FechaCreacion.Month];
					m.cant++; m.total += p.Total;

					// Detalle de días del mes (solo si se pidió mesDetalle)
					if (mesDetalle.HasValue && p.FechaCreacion.Month == mesDetalle.Value)
					{
						ref var porDiaMes = ref CollectionsMarshal.GetValueRefOrAddDefault(porDiaMesDict, fecha, out _);
						porDiaMes.cant++; porDiaMes.total += p.Total;
					}
				}

				// Top productos (todo el histórico)
				foreach (var item in p.Items)
				{
					ref var tp = ref CollectionsMarshal.GetValueRefOrAddDefault(topProductosDict, item.Nombre, out _);
					tp.cant += item.Cantidad; tp.total += item.Subtotal;
				}
			}

			// --- Construcción de DTOs a partir de los acumuladores ---
			var ventasPorHora = Enumerable.Range(0, 24)
				.Select(h => new VentasPorHora
				{
					Hora = h,
					Etiqueta = $"{h:D2}:00",
					CantidadPedidos = horaCounts[h].cant,
					TotalVendido = horaCounts[h].total
				})
				.ToList();

			var topProductos = topProductosDict
				.Select(kv => new ProductoMasVendido { Nombre = kv.Key, CantidadVendida = kv.Value.cant, TotalGenerado = kv.Value.total })
				.OrderByDescending(p => p.CantidadVendida)
				.Take(10)
				.ToList();

			var diasOrdenados = new[]
			{
				DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
				DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday
			};
			var nombresDias = new Dictionary<DayOfWeek, string>
			{
				[DayOfWeek.Monday] = "Lunes",
				[DayOfWeek.Tuesday] = "Martes",
				[DayOfWeek.Wednesday] = "Miércoles",
				[DayOfWeek.Thursday] = "Jueves",
				[DayOfWeek.Friday] = "Viernes",
				[DayOfWeek.Saturday] = "Sábado",
				[DayOfWeek.Sunday] = "Domingo"
			};
			var ventasPorDiaSemana = diasOrdenados
				.Select(dia => new VentasPorDiaSemana
				{
					DiaSemana = dia,
					Etiqueta = nombresDias[dia],
					CantidadPedidos = diaSemanaCounts[(int)dia].cant,
					TotalVendido = diaSemanaCounts[(int)dia].total
				})
				.ToList();

			var historialPorDia = porDiaDict
				.Select(kv => new VentasPorDia { Fecha = kv.Key, Etiqueta = kv.Key.ToString("dd/MM/yyyy"), CantidadPedidos = kv.Value.cant, TotalVendido = kv.Value.total })
				.OrderByDescending(v => v.Fecha)
				.ToList();

			var nombresMesesCortos = new[] { "Ene", "Feb", "Mar", "Abr", "May", "Jun", "Jul", "Ago", "Sep", "Oct", "Nov", "Dic" };
			var ventasPorMes = Enumerable.Range(1, 12)
				.Select(m => new VentasPorMes { Mes = m, Etiqueta = nombresMesesCortos[m - 1], CantidadPedidos = porMesCounts[m].cant, TotalVendido = porMesCounts[m].total })
				.ToList();

			var diasDelMesSeleccionado = mesDetalle.HasValue
				? porDiaMesDict
					.Select(kv => new VentasPorDia { Fecha = kv.Key, Etiqueta = kv.Key.ToString("dd/MM/yyyy"), CantidadPedidos = kv.Value.cant, TotalVendido = kv.Value.total })
					.OrderBy(v => v.Fecha)
					.ToList()
				: new List<VentasPorDia>();

			return new ResumenMapaCalor
			{
				VentasPorHora = ventasPorHora,
				TopProductos = topProductos,
				VentasPorDiaSemana = ventasPorDiaSemana,
				HistorialPorDia = historialPorDia,
				FechaSeleccionada = fechaSeleccionada,
				TotalPedidos = totalPedidosDelDia,
				TotalVendido = totalVendidoDelDia,

				VentasPorMes = ventasPorMes,
				AnioMesesSeleccionado = anioMesesSeleccionado,
				DiasDelMesSeleccionado = diasDelMesSeleccionado,
				MesDetalleSeleccionado = mesDetalle,

				InicioSemana = inicioSemana,
				FinSemana = finSemana,
				SemanaOffset = semanaOffset,
				SemanaHistoricoCompleto = semanaHistoricoCompleto
			};
		}
	}
}