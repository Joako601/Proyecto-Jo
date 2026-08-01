using Microsoft.EntityFrameworkCore;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Infrastructure.Persistence.EfCore.Repositories
{
	public class EfPedidoRepository : IPedidoRepository
	{
		private readonly ProyectoJoDbContext _context;

		public EfPedidoRepository(ProyectoJoDbContext context)
		{
			_context = context;
		}

		public async Task<List<Pedido>> ObtenerTodosAsync() =>
			await _context.Pedidos.Include(p => p.Items).AsNoTracking().ToListAsync();

		public async Task<List<Pedido>> ObtenerActivosAsync() =>
			await _context.Pedidos
				.Include(p => p.Items)
				.AsNoTracking()
				.Where(p => p.Estado == EstadoPedido.Pendiente || p.Estado == EstadoPedido.Preparado)
				.OrderBy(p => p.FechaCreacion)
				.ToListAsync();

		public async Task<List<Pedido>> ObtenerDelDiaAsync(DateTime desde) =>
			await _context.Pedidos
				.Include(p => p.Items)
				.AsNoTracking()
				.Where(p => p.Estado != EstadoPedido.Cancelado && p.FechaCreacion >= desde)
				.OrderByDescending(p => p.FechaCreacion)
				.ToListAsync();

		public async Task<Pedido?> ObtenerPorIdAsync(int id) =>
			await _context.Pedidos.Include(p => p.Items).AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);

		public async Task<Pedido> GuardarAsync(Pedido pedido)
		{
			_context.Pedidos.Add(pedido);
			await _context.SaveChangesAsync();
			return pedido;
		}

		public async Task<Pedido?> ActualizarAsync(Pedido pedido)
		{
			var existente = await _context.Pedidos.Include(p => p.Items).FirstOrDefaultAsync(p => p.Id == pedido.Id);
			if (existente is null) return null;

			_context.Entry(existente).CurrentValues.SetValues(pedido);
			existente.Items = pedido.Items;

			await _context.SaveChangesAsync();
			return existente;
		}

		public async Task<(Pedido? Anterior, Pedido? Actualizado, string? MotivoRechazo)> CambiarEstadoAtomicoAsync(
			int id,
			EstadoPedido nuevoEstado,
			Func<Pedido, Task<string?>>? validarAntesDeAplicar = null)
		{
			await using var transaction = await _context.Database.BeginTransactionAsync();

			var pedido = await _context.Pedidos
				.FromSqlInterpolated($"SELECT * FROM pedidos WHERE id = {id} FOR UPDATE")
				.Include(p => p.Items)
				.FirstOrDefaultAsync();

			if (pedido is null)
			{
				await transaction.RollbackAsync();
				return (null, null, null);
			}

			var anteriorSnapshot = new Pedido
			{
				Id = pedido.Id,
				Mesa = pedido.Mesa,
				Estado = pedido.Estado,
				FechaCreacion = pedido.FechaCreacion,
				Items = pedido.Items
					.Select(i => new ItemPedido { ItemId = i.ItemId, Nombre = i.Nombre, Cantidad = i.Cantidad, PrecioUnitario = i.PrecioUnitario })
					.ToList()
			};

			if (validarAntesDeAplicar is not null)
			{
				var motivoRechazo = await validarAntesDeAplicar(anteriorSnapshot);
				if (motivoRechazo is not null)
				{
					await transaction.RollbackAsync();
					return (anteriorSnapshot, null, motivoRechazo);
				}
			}

			pedido.Estado = nuevoEstado;
			await _context.SaveChangesAsync();
			await transaction.CommitAsync();

			return (anteriorSnapshot, pedido, null);
		}
	}
}
