using Microsoft.EntityFrameworkCore;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Infrastructure.Persistence.EfCore.Repositories
{
	public class EfAuditoriaRepository : IAuditoriaRepository
	{
		private readonly ProyectoJoDbContext _context;

		public EfAuditoriaRepository(ProyectoJoDbContext context)
		{
			_context = context;
		}

		public List<RegistroAuditoria> ObtenerTodos() => _context.RegistrosAuditoria.AsNoTracking().ToList();

		public (List<RegistroAuditoria> Items, int Total) ObtenerPaginado(
			string? modulo, DateTime? desde, DateTime? hasta, int pagina, int porPagina)
		{
			var query = _context.RegistrosAuditoria.AsNoTracking();

			if (!string.IsNullOrWhiteSpace(modulo))
				query = query.Where(r => EF.Functions.ILike(r.Modulo, modulo));

			if (desde.HasValue)
				query = query.Where(r => r.FechaHora.Date >= desde.Value.Date);

			if (hasta.HasValue)
				query = query.Where(r => r.FechaHora.Date <= hasta.Value.Date);

			var total = query.Count();

			var items = query
				.OrderByDescending(r => r.FechaHora)
				.Skip((pagina - 1) * porPagina)
				.Take(porPagina)
				.ToList();

			return (items, total);
		}

		public void Guardar(RegistroAuditoria registro)
		{
			_context.RegistrosAuditoria.Add(registro);
			_context.SaveChanges();
		}
	}
}
