using ProyectoJo.Application.Ports.In;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Domain.Entities;
using ProyectoJo.Application.DTOs;
using Microsoft.Extensions.Logging;

namespace ProyectoJo.Application.UseCases
{
	public class PedidoUseCase : IPedidoService
	{
		private readonly IPedidoRepository _repository;
		private readonly IFinanzaService _finanzaService;
		private readonly IPedidoNotificador _notificador;
		private readonly IProductoService _productoService;
		private readonly IPromocionService _promocionService;
		private readonly ILogger<PedidoUseCase> _logger;

		public PedidoUseCase(
			IPedidoRepository repository,
			IFinanzaService finanzaService,
			IPedidoNotificador notificador,
			IProductoService productoService,
			IPromocionService promocionService,
			ILogger<PedidoUseCase> logger)
		{
			_repository = repository;
			_finanzaService = finanzaService;
			_notificador = notificador;
			_productoService = productoService;
			_promocionService = promocionService;
			_logger = logger;
		}

		public async Task<List<Pedido>> ObtenerPendientesAsync()
		{
			var todos = await _repository.ObtenerTodosAsync();
			return todos.Where(p => p.Estado == EstadoPedido.Pendiente).ToList();
		}

		public async Task<Pedido?> ObtenerPorIdAsync(int id)
		{
			return await _repository.ObtenerPorIdAsync(id);
		}

		public async Task<ResultadoCrearPedido> CrearAsync(Pedido pedido)
		{
			var lineasValidas = new List<ItemPedido>();
			var lineasDescartadas = new List<LineaDescartada>();

			foreach (var linea in pedido.Items)
			{
				if (linea.Cantidad <= 0)
				{
					lineasDescartadas.Add(new LineaDescartada { ItemId = linea.ItemId, Nombre = linea.Nombre, Motivo = "Cantidad inválida" });
					continue;
				}

				var item = _productoService.ObtenerPorId(linea.ItemId);
				if (item is null || !item.Activo)
				{
					lineasDescartadas.Add(new LineaDescartada { ItemId = linea.ItemId, Nombre = linea.Nombre, Motivo = "Ya no está disponible en el menú" });
					continue;
				}

				if (item.Agotado)
				{
					lineasDescartadas.Add(new LineaDescartada { ItemId = linea.ItemId, Nombre = linea.Nombre, Motivo = "Sin stock en este momento" });
					continue;
				}

				linea.PrecioUnitario = _promocionService.CalcularPrecioFinal(item);
				lineasValidas.Add(linea);
			}

			if (lineasValidas.Count == 0)
				throw new InvalidOperationException("Ninguno de los productos del pedido está disponible. No se creó el pedido.");

			pedido.Items = lineasValidas;
			pedido.Estado = EstadoPedido.Pendiente;
			pedido.FechaCreacion = DateTime.UtcNow;
			var creado = await _repository.GuardarAsync(pedido);

			try { await _notificador.NotificarCreadoAsync(creado); }
			catch (Exception ex) { _logger.LogError(ex, "Error notificando creación del Pedido #{PedidoId}", creado.Id); }

			return new ResultadoCrearPedido { Pedido = creado, LineasDescartadas = lineasDescartadas };
		}

		public async Task<Pedido?> CambiarEstadoAsync(int id, EstadoPedido nuevoEstado)
		{
			var pedidoAntes = await _repository.ObtenerPorIdAsync(id);
			if (pedidoAntes is null) return null;

			var yaEstabaPagado = pedidoAntes.Estado == EstadoPedido.Pagado;
			var actualizado = await _repository.CambiarEstadoAtomicoAsync(id, nuevoEstado);

			if (actualizado is not null && nuevoEstado == EstadoPedido.Pagado && !yaEstabaPagado)
			{
				try
				{
					_finanzaService.RegistrarMovimiento(new Finanza
					{
						Monto = actualizado.Total,
						Tipo = TipoMovimiento.Ingreso,
						Categoria = "Ventas",
						Descripcion = $"Pedido #{actualizado.Id} — Mesa {actualizado.Mesa}",
						Fecha = DateTime.UtcNow
					}, "Sistema (Pedido)");
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Error registrando finanza para el Pedido #{PedidoId}", id);
				}
			}

			if (actualizado is not null)
			{
				try
				{
					await _notificador.NotificarEstadoCambiadoAsync(actualizado);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Error notificando cambio de estado del Pedido #{PedidoId}", id);
				}
			}

			return actualizado;
		}

		public async Task<List<Pedido>> ObtenerParaCocinaAsync()
		{
			var todos = await _repository.ObtenerTodosAsync();
			return todos
				.Where(p => p.Estado == EstadoPedido.Pendiente || p.Estado == EstadoPedido.Preparado)
				.OrderBy(p => p.FechaCreacion)
				.ToList();
		}

		public async Task<List<Pedido>> ObtenerParaRecepcionAsync()
		{
			var todos = await _repository.ObtenerTodosAsync();
			return todos
				.Where(p => p.Estado != EstadoPedido.Cancelado)
				.OrderByDescending(p => p.FechaCreacion)
				.ToList();
		}

		public async Task<ResumenMapaCalor> ObtenerMapaCalorAsync(
			DateTime? desde = null,
			DateTime? hasta = null,
			bool semanaHistoricoCompleto = true,
			int semanaOffset = 0,
			int? anioMeses = null,
			int? mesDetalle = null)
		{
			var todos = await _repository.ObtenerTodosAsync();
			var pagados = todos.Where(p => p.Estado == EstadoPedido.Pagado).ToList();

			var hoy = DateTime.UtcNow.Date;
			var fechaSeleccionada = (desde ?? hoy).Date;

			// --- Pedidos por hora del día seleccionado ---
			var pedidosDelDia = pagados
				.Where(p => p.FechaCreacion.Date == fechaSeleccionada)
				.ToList();

			var ventasPorHoraAgrupado = pedidosDelDia
				.GroupBy(p => p.FechaCreacion.Hour)
				.Select(g => new VentasPorHora
				{
					Hora = g.Key,
					Etiqueta = $"{g.Key:D2}:00",
					CantidadPedidos = g.Count(),
					TotalVendido = g.Sum(p => p.Total)
				})
				.ToList();

			var horasCompletas = Enumerable.Range(0, 24)
				.Select(h => ventasPorHoraAgrupado.FirstOrDefault(v => v.Hora == h)
					?? new VentasPorHora { Hora = h, Etiqueta = $"{h:D2}:00" })
				.OrderBy(v => v.Hora)
				.ToList();

			// --- Top productos (histórico completo) ---
			var topProductos = pagados
				.SelectMany(p => p.Items)
				.GroupBy(i => i.Nombre)
				.Select(g => new ProductoMasVendido
				{
					Nombre = g.Key,
					CantidadVendida = g.Sum(i => i.Cantidad),
					TotalGenerado = g.Sum(i => i.Subtotal)
				})
				.OrderByDescending(p => p.CantidadVendida)
				.Take(10)
				.ToList();

			// --- Ventas por día de la semana ---
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

			int diffHastaLunes = ((int)hoy.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
			var lunesSemanaActual = hoy.AddDays(-diffHastaLunes);
			var inicioSemana = lunesSemanaActual.AddDays(7 * semanaOffset);
			var finSemana = inicioSemana.AddDays(6);

			var pedidosParaDiaSemana = semanaHistoricoCompleto
				? pagados
				: pagados.Where(p => p.FechaCreacion.Date >= inicioSemana && p.FechaCreacion.Date <= finSemana).ToList();

			var ventasPorDiaSemanaAgrupado = pedidosParaDiaSemana
				.GroupBy(p => p.FechaCreacion.DayOfWeek)
				.ToDictionary(g => g.Key, g => g.ToList());

			var ventasPorDiaSemana = diasOrdenados
				.Select(dia => new VentasPorDiaSemana
				{
					DiaSemana = dia,
					Etiqueta = nombresDias[dia],
					CantidadPedidos = ventasPorDiaSemanaAgrupado.ContainsKey(dia) ? ventasPorDiaSemanaAgrupado[dia].Count : 0,
					TotalVendido = ventasPorDiaSemanaAgrupado.ContainsKey(dia) ? ventasPorDiaSemanaAgrupado[dia].Sum(p => p.Total) : 0
				})
				.ToList();

			// --- Historial día por día ---
			var historialPorDia = pagados
				.GroupBy(p => p.FechaCreacion.Date)
				.Select(g => new VentasPorDia
				{
					Fecha = g.Key,
					Etiqueta = g.Key.ToString("dd/MM/yyyy"),
					CantidadPedidos = g.Count(),
					TotalVendido = g.Sum(p => p.Total)
				})
				.OrderByDescending(v => v.Fecha)
				.ToList();

			// --- Ventas por mes del año seleccionado ---
			var anioMesesSeleccionado = anioMeses ?? hoy.Year;

			var nombresMesesCortos = new[]
			{
				"Ene", "Feb", "Mar", "Abr", "May", "Jun",
				"Jul", "Ago", "Sep", "Oct", "Nov", "Dic"
			};

			var pedidosDelAnio = pagados
				.Where(p => p.FechaCreacion.Year == anioMesesSeleccionado)
				.ToList();

			var ventasPorMesAgrupado = pedidosDelAnio
				.GroupBy(p => p.FechaCreacion.Month)
				.ToDictionary(g => g.Key, g => g.ToList());

			var ventasPorMes = Enumerable.Range(1, 12)
				.Select(m => new VentasPorMes
				{
					Mes = m,
					Etiqueta = nombresMesesCortos[m - 1],
					CantidadPedidos = ventasPorMesAgrupado.ContainsKey(m) ? ventasPorMesAgrupado[m].Count : 0,
					TotalVendido = ventasPorMesAgrupado.ContainsKey(m) ? ventasPorMesAgrupado[m].Sum(p => p.Total) : 0
				})
				.ToList();

			// --- Detalle de días del mes seleccionado ---
			var diasDelMesSeleccionado = new List<VentasPorDia>();
			if (mesDetalle.HasValue)
			{
				diasDelMesSeleccionado = pagados
					.Where(p => p.FechaCreacion.Month == mesDetalle.Value && p.FechaCreacion.Year == anioMesesSeleccionado)
					.GroupBy(p => p.FechaCreacion.Date)
					.Select(g => new VentasPorDia
					{
						Fecha = g.Key,
						Etiqueta = g.Key.ToString("dd/MM/yyyy"),
						CantidadPedidos = g.Count(),
						TotalVendido = g.Sum(p => p.Total)
					})
					.OrderBy(v => v.Fecha)
					.ToList();
			}

			return new ResumenMapaCalor
			{
				VentasPorHora = horasCompletas,
				TopProductos = topProductos,
				VentasPorDiaSemana = ventasPorDiaSemana,
				HistorialPorDia = historialPorDia,
				FechaSeleccionada = fechaSeleccionada,
				TotalPedidos = pedidosDelDia.Count,
				TotalVendido = pedidosDelDia.Sum(p => p.Total),

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