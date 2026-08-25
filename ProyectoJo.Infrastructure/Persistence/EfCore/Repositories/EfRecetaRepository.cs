using Microsoft.EntityFrameworkCore;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Infrastructure.Persistence.EfCore.Repositories
{
	public class EfRecetaRepository : IRecetaRepository
	{
		private readonly ProyectoJoDbContext _context;

		public EfRecetaRepository(ProyectoJoDbContext context)
		{
			_context = context;
		}

		public List<Receta> ObtenerTodas() => _context.Recetas.Include(r => r.Ingredientes).AsNoTracking().OrderBy(r => r.Id).ToList();

		public Receta? ObtenerPorId(int id) =>
			_context.Recetas.Include(r => r.Ingredientes).AsNoTracking().FirstOrDefault(r => r.Id == id);

		public Receta? ObtenerPorItemId(int itemId) =>
			_context.Recetas.Include(r => r.Ingredientes).AsNoTracking().FirstOrDefault(r => r.ItemId == itemId);

		public void Agregar(Receta receta)
		{
			_context.Recetas.Add(receta);
			_context.SaveChanges();
		}

		public bool Editar(Receta receta)
		{
			var existente = _context.Recetas.Include(r => r.Ingredientes).FirstOrDefault(r => r.Id == receta.Id);
			if (existente is null) return false;

			_context.Entry(existente).CurrentValues.SetValues(receta);
			existente.Ingredientes = receta.Ingredientes;

			_context.SaveChanges();
			return true;
		}

		public bool Eliminar(int id)
		{
			var receta = _context.Recetas.FirstOrDefault(r => r.Id == id);
			if (receta is null) return false;

			_context.Recetas.Remove(receta);
			_context.SaveChanges();
			return true;
		}
	}
}
