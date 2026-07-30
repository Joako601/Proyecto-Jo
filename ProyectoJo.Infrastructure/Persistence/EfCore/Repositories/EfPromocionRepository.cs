using Microsoft.EntityFrameworkCore;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Infrastructure.Persistence.EfCore.Repositories
{
	public class EfPromocionRepository : IPromocionRepository
	{
		private readonly ProyectoJoDbContext _context;

		public EfPromocionRepository(ProyectoJoDbContext context)
		{
			_context = context;
		}

		public IEnumerable<Promocion> ObtenerTodas() => _context.Promociones.AsNoTracking().ToList();

		public Promocion? ObtenerPorId(int id) => _context.Promociones.AsNoTracking().FirstOrDefault(p => p.Id == id);

		public void Agregar(Promocion promocion)
		{
			_context.Promociones.Add(promocion);
			_context.SaveChanges();
		}

		public bool Editar(Promocion promocion)
		{
			if (!_context.Promociones.Any(p => p.Id == promocion.Id)) return false;

			_context.Promociones.Update(promocion);
			_context.SaveChanges();
			return true;
		}

		public bool Eliminar(int id)
		{
			var promocion = _context.Promociones.Find(id);
			if (promocion is null) return false;

			_context.Promociones.Remove(promocion);
			_context.SaveChanges();
			return true;
		}

		public bool ToggleActiva(int id)
		{
			var promocion = _context.Promociones.Find(id);
			if (promocion is null) return false;

			promocion.Activa = !promocion.Activa;
			_context.SaveChanges();
			return true;
		}
	}
}
