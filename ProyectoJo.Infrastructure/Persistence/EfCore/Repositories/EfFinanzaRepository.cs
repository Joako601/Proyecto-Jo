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

		public List<Finanza> ObtenerPorFecha(DateTime desde, DateTime hasta) =>
			_context.Finanzas
				.AsNoTracking()
				.Where(f => f.Fecha.Date >= desde.Date && f.Fecha.Date <= hasta.Date)
				.ToList();

		public (List<Finanza> Items, int Total) ObtenerPaginado(int mes, int anio, int pagina, int porPagina)
		{
			var inicio = new DateTime(anio, mes, 1);
			var fin = inicio.AddMonths(1);

			var query = _context.Finanzas
				.AsNoTracking()
				.Where(f => f.Fecha >= inicio && f.Fecha < fin);

			var total = query.Count();

			var items = query
				.OrderByDescending(f => f.Fecha)
				.Skip((pagina - 1) * porPagina)
				.Take(porPagina)
				.ToList();

			return (items, total);
		}

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
