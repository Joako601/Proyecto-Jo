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

		public void Guardar(RegistroAuditoria registro)
		{
			_context.RegistrosAuditoria.Add(registro);
			_context.SaveChanges();
		}
	}
}
