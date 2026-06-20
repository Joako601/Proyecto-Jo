using ProyectoJo.Application.Ports.In;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Application.UseCases
{
	public class PedidoUseCase : IPedidoService
	{
		private readonly IPedidoRepository _repository;

		public PedidoUseCase(IPedidoRepository repository)
		{
			_repository = repository;
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

			pedido.Estado = nuevoEstado;
			return await _repository.ActualizarAsync(pedido);
		}
	}
}