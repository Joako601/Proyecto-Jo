using Microsoft.EntityFrameworkCore;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Infrastructure.Persistence.EfCore.Repositories
{
	public class EfFinanzaRepository : IFinanzaRepository
	{
		private readonly ProyectoJoDbContext _context;

		public EfFinanzaRepository(ProyectoJoDbContext context)
		{
			_context = context;
		}

		public List<Finanza> ObtenerTodos() => _context.Finanzas.AsNoTracking().ToList();

		public Finanza? ObtenerPorId(int id) => _context.Finanzas.AsNoTracking().FirstOrDefault(f => f.Id == id);

		public void Guardar(Finanza finanza)
		{
			_context.Finanzas.Add(finanza);
			_context.SaveChanges();
		}

		public void Actualizar(Finanza finanza)
		{
			_context.Finanzas.Update(finanza);
			_context.SaveChanges();
		}

		public void Eliminar(int id)
		{
			var finanza = _context.Finanzas.Find(id);
			if (finanza is null) return;

			_context.Finanzas.Remove(finanza);
			_context.SaveChanges();
		}
	}
}
