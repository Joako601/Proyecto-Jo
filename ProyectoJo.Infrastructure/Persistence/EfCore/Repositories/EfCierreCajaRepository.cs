using Microsoft.EntityFrameworkCore;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Infrastructure.Persistence.EfCore.Repositories
{
	public class EfCierreCajaRepository : ICierreCajaRepository
	{
		private readonly ProyectoJoDbContext _context;

		public EfCierreCajaRepository(ProyectoJoDbContext context)
		{
			_context = context;
		}

		public List<CierreCaja> ObtenerTodos() => _context.CierresCaja.AsNoTracking().ToList();

		public CierreCaja? ObtenerPorId(int id) => _context.CierresCaja.AsNoTracking().FirstOrDefault(c => c.Id == id);

		public void Guardar(CierreCaja cierreCaja)
		{
			_context.CierresCaja.Add(cierreCaja);
			_context.SaveChanges();
		}

		public void Actualizar(CierreCaja cierreCaja)
		{
			_context.CierresCaja.Update(cierreCaja);
			_context.SaveChanges();
		}

		public bool IntentarAbrir(CierreCaja nuevaCaja)
		{
			using var transaction = _context.Database.BeginTransaction();

			if (_context.CierresCaja.Any(c => c.Estado == EstadoCaja.Abierta))
			{
				transaction.Rollback();
				return false;
			}

			_context.CierresCaja.Add(nuevaCaja);
			_context.SaveChanges();
			transaction.Commit();
			return true;
		}
	}
}
