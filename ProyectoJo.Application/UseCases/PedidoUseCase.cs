using ProyectoJo.Application.Ports.In;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Application.UseCases
{
	public class PedidoUseCase : IPedidoService
	{
		private readonly IPedidoRepository _repository;
		private readonly IFinanzaService _finanzaService;

		public PedidoUseCase(IPedidoRepository repository, IFinanzaService finanzaService)
		{
			_repository = repository;
			_finanzaService = finanzaService;
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

		public async Task<Pedido> CrearAsync(Pedido pedido)
		{
			pedido.Estado = EstadoPedido.Pendiente;
			pedido.FechaCreacion = DateTime.UtcNow;
			return await _repository.GuardarAsync(pedido);
		}

		public async Task<Pedido?> CambiarEstadoAsync(int id, EstadoPedido nuevoEstado)
		{
			var pedido = await _repository.ObtenerPorIdAsync(id);
			if (pedido is null) return null;

			var yaEstabaPagado = pedido.Estado == EstadoPedido.Pagado;
			pedido.Estado = nuevoEstado;
			var actualizado = await _repository.ActualizarAsync(pedido);

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
					});
				}
				catch (Exception ex)
				{
					Console.Error.WriteLine($"[Pedido #{id}] Error registrando finanza: {ex.Message}");
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
	}
}