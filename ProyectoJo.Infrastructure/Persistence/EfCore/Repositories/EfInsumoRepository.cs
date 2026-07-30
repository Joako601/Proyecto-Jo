using Microsoft.EntityFrameworkCore;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Infrastructure.Persistence.EfCore.Repositories
{
	public class EfInsumoRepository : IInsumoRepository
	{
		private readonly ProyectoJoDbContext _context;

		public EfInsumoRepository(ProyectoJoDbContext context)
		{
			_context = context;
		}

		public List<Insumo> ObtenerTodos() => _context.Insumos.AsNoTracking().ToList();

		public Insumo? ObtenerPorId(int id) => _context.Insumos.AsNoTracking().FirstOrDefault(i => i.Id == id);

		public void Agregar(Insumo insumo)
		{
			_context.Insumos.Add(insumo);
			_context.SaveChanges();
		}

		public void AgregarRango(IEnumerable<Insumo> insumos)
		{
			var lista = insumos as ICollection<Insumo> ?? insumos.ToList();
			if (lista.Count == 0) return;

			_context.Insumos.AddRange(lista);
			_context.SaveChanges();
		}

		public bool Editar(Insumo insumo)
		{
			if (!_context.Insumos.Any(i => i.Id == insumo.Id)) return false;

			_context.Insumos.Update(insumo);
			_context.SaveChanges();
			return true;
		}

		public bool Eliminar(int id)
		{
			var insumo = _context.Insumos.Find(id);
			if (insumo is null) return false;

			_context.Insumos.Remove(insumo);
			_context.SaveChanges();
			return true;
		}

		public async Task<(bool Exitoso, List<FaltanteInsumo> Faltantes)> DescontarAtomicoAsync(
			Dictionary<int, decimal> consumoPorInsumoId)
		{
			await using var transaction = await _context.Database.BeginTransactionAsync();

			var ids = consumoPorInsumoId.Keys.ToList();
			var insumos = await _context.Insumos
				.FromSqlInterpolated($"SELECT * FROM insumos WHERE id = ANY({ids}) FOR UPDATE")
				.ToListAsync();

			var faltantes = new List<FaltanteInsumo>();

			foreach (var (insumoId, necesario) in consumoPorInsumoId)
			{
				var insumo = insumos.FirstOrDefault(i => i.Id == insumoId);
				if (insumo is null || insumo.StockActual < necesario)
				{
					faltantes.Add(new FaltanteInsumo
					{
						InsumoId = insumoId,
						Nombre = insumo?.Nombre ?? $"Insumo #{insumoId}",
						Necesario = necesario,
						Disponible = insumo?.StockActual ?? 0
					});
				}
			}

			if (faltantes.Count > 0)
			{
				await transaction.RollbackAsync();
				return (false, faltantes);
			}

			foreach (var (insumoId, necesario) in consumoPorInsumoId)
			{
				var insumo = insumos.First(i => i.Id == insumoId);
				insumo.StockActual -= necesario;
			}

			await _context.SaveChangesAsync();
			await transaction.CommitAsync();
			return (true, new List<FaltanteInsumo>());
		}

		public async Task<Insumo?> ReponerAtomicoAsync(int id, decimal cantidad)
		{
			await using var transaction = await _context.Database.BeginTransactionAsync();

			var insumo = await _context.Insumos
				.FromSqlInterpolated($"SELECT * FROM insumos WHERE id = {id} FOR UPDATE")
				.FirstOrDefaultAsync();

			if (insumo is null)
			{
				await transaction.RollbackAsync();
				return null;
			}

			insumo.StockActual += cantidad;
			await _context.SaveChangesAsync();
			await transaction.CommitAsync();
			return insumo;
		}
	}
}
