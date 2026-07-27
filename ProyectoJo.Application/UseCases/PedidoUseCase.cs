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
			Func<Pedido, Task<string?>> validador = async pedido =>
			{
				// Se valida contra el estado real leído dentro del lock del
				// repositorio, no contra una copia leída antes: dos requests
				// casi simultáneas (Cocina y Recepción) no pueden colarse una
				// transición inválida aprovechando una ventana de carrera.
				if (!pedido.PuedeTransicionarA(nuevoEstado))
					return $"No se puede cambiar el pedido de '{pedido.Estado}' a '{nuevoEstado}'.";

				if (nuevoEstado == EstadoPedido.Preparado)
				{
					return await _insumoService.VerificarYDescontarAsync(
						pedido.Items,
						itemId => _recetaService.ObtenerPorItemId(itemId));
				}

				return null;
			};

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
	}
}