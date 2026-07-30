using Microsoft.EntityFrameworkCore;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Infrastructure.Persistence.EfCore.Repositories
{
	public class EfOpinionRepository : IOpinionRepository
	{
		private readonly ProyectoJoDbContext _context;

		public EfOpinionRepository(ProyectoJoDbContext context)
		{
			_context = context;
		}

		public List<OpinionCliente> ObtenerTodas() => _context.Opiniones.AsNoTracking().ToList();

		public OpinionCliente? ObtenerPorId(int id) => _context.Opiniones.AsNoTracking().FirstOrDefault(o => o.Id == id);

		public void Agregar(OpinionCliente opinion)
		{
			_context.Opiniones.Add(opinion);
			_context.SaveChanges();
		}

		public bool Editar(OpinionCliente opinion)
		{
			if (!_context.Opiniones.Any(o => o.Id == opinion.Id)) return false;

			_context.Opiniones.Update(opinion);
			_context.SaveChanges();
			return true;
		}

		public bool Eliminar(int id)
		{
			var opinion = _context.Opiniones.Find(id);
			if (opinion is null) return false;

			_context.Opiniones.Remove(opinion);
			_context.SaveChanges();
			return true;
		}
	}
}
