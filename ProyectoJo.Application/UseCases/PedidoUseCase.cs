using ProyectoJo.Application.Ports.In;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Domain.Entities;
using ProyectoJo.Application.DTOs;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;

namespace ProyectoJo.Application.UseCases
{
	public class PedidoUseCase : IPedidoService
	{
		private readonly IPedidoRepository _repository;
		private readonly IFinanzaService _finanzaService;
		private readonly IPedidoNotificador _notificador;
		private readonly IProductoService _productoService;
		private readonly IPromocionService _promocionService;
		private readonly IInsumoService _insumoService;
		private readonly IRecetaService _recetaService;
		private readonly ILogger<PedidoUseCase> _logger;

		public PedidoUseCase(
			IPedidoRepository repository,
			IFinanzaService finanzaService,
			IPedidoNotificador notificador,
			IProductoService productoService,
			IPromocionService promocionService,
			IInsumoService insumoService,
			IRecetaService recetaService,
			ILogger<PedidoUseCase> logger)
		{
			_repository = repository;
			_finanzaService = finanzaService;
			_notificador = notificador;
			_productoService = productoService;
			_promocionService = promocionService;
			_insumoService = insumoService;
			_recetaService = recetaService;
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
			var lineasAjustadas = new List<LineaAjustada>();

			// 1 sola lectura de cada catálogo (y ahora cacheada, casi gratis) en vez de N por línea
			var menu = _productoService.ObtenerTodos().ToDictionary(i => i.Id);
			var promosVigentes = _promocionService.ObtenerVigentes().ToList();

			foreach (var linea in pedido.Items)
			{
				if (linea.Cantidad <= 0)
				{
					lineasDescartadas.Add(new LineaDescartada { ItemId = linea.ItemId, Nombre = linea.Nombre, Motivo = "Cantidad inválida" });
					continue;
				}

				if (!menu.TryGetValue(linea.ItemId, out var item) || !item.Activo)
				{
					lineasDescartadas.Add(new LineaDescartada { ItemId = linea.ItemId, Nombre = linea.Nombre, Motivo = "Ya no está disponible en el menú" });
					continue;
				}

				if (item.Agotado)
				{
					lineasDescartadas.Add(new LineaDescartada { ItemId = linea.ItemId, Nombre = linea.Nombre, Motivo = "Sin stock en este momento" });
					continue;
				}

				// Verificación autoritativa de stock por ingredientes: el cliente puede
				// haber mandado una cantidad que ya no es válida (el stock pudo cambiar
				// entre que Recepción armó el carrito y apretó "Crear pedido").
				var stockMaximo = _insumoService.ObtenerMaximoDisponible(item);
				if (stockMaximo.HasValue)
				{
					if (stockMaximo.Value <= 0)
					{
						lineasDescartadas.Add(new LineaDescartada { ItemId = linea.ItemId, Nombre = linea.Nombre, Motivo = "Sin stock de ingredientes" });
						continue;
					}

					if (linea.Cantidad > stockMaximo.Value)
					{
						lineasAjustadas.Add(new LineaAjustada
						{
							ItemId = linea.ItemId,
							Nombre = linea.Nombre,
							CantidadSolicitada = linea.Cantidad,
							CantidadFinal = stockMaximo.Value
						});
						linea.Cantidad = stockMaximo.Value;
					}
				}

				linea.PrecioUnitario = CalcularPrecioFinalEnMemoria(item, promosVigentes);
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

			return new ResultadoCrearPedido { Pedido = creado, LineasDescartadas = lineasDescartadas, LineasAjustadas = lineasAjustadas };
		}

		private static decimal CalcularPrecioFinalEnMemoria(Item item, List<Promocion> promosVigentes)
		{
			var promo = promosVigentes
				.Where(p => (p.ItemIds != null && p.ItemIds.Contains(item.Id))
						 && p.TipoDescuento != TipoDescuento.Ninguno && p.ValorDescuento.HasValue)
				.OrderByDescending(p => p.Id)
				.FirstOrDefault();

			if (promo == null) return item.Precio;

			decimal precioFinal = promo.TipoDescuento switch
			{
				TipoDescuento.Porcentaje => item.Precio - (item.Precio * (promo.ValorDescuento!.Value / 100m)),
				TipoDescuento.MontoFijo => item.Precio - promo.ValorDescuento!.Value,
				_ => item.Precio
			};

			return precioFinal < 0 ? 0 : Math.Round(precioFinal, 2);
		}

		public async Task<ResultadoCambiarEstado> CambiarEstadoAsync(int id, EstadoPedido nuevoEstado)
		{
			Func<Pedido, Task<string?>>? validador = null;

			if (nuevoEstado == EstadoPedido.Preparado)
			{
				validador = pedido => _insumoService.VerificarYDescontarAsync(
					pedido.Items,
					itemId => _recetaService.ObtenerPorItemId(itemId));
			}

			var (pedidoAntes, actualizado, motivoRechazo) =
				await _repository.CambiarEstadoAtomicoAsync(id, nuevoEstado, validador);

			if (pedidoAntes is null)
				return new ResultadoCambiarEstado { NoEncontrado = true };

			if (motivoRechazo is not null)
				return new ResultadoCambiarEstado { Exitoso = false, MotivoRechazo = motivoRechazo, Pedido = pedidoAntes };

			var yaEstabaPagado = pedidoAntes.Estado == EstadoPedido.Pagado;

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
				try { await _notificador.NotificarEstadoCambiadoAsync(actualizado); }
				catch (Exception ex) { _logger.LogError(ex, "Error notificando cambio de estado del Pedido #{PedidoId}", id); }
			}

			return new ResultadoCambiarEstado { Exitoso = true, Pedido = actualizado };
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