using Microsoft.EntityFrameworkCore;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Infrastructure.Persistence.EfCore.Repositories
{
	public class EfAdministradorRepository : IAdministradorRepository
	{
		private readonly ProyectoJoDbContext _context;

		public EfAdministradorRepository(ProyectoJoDbContext context)
		{
			_context = context;
		}

		public async Task<List<Administrador>> ObtenerTodosAsync() =>
			await _context.Administradores.AsNoTracking().ToListAsync();

		public async Task<Administrador?> ObtenerPorIdAsync(int id) =>
			await _context.Administradores.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);

		public async Task<Administrador?> ObtenerPorUsuarioAsync(string usuario) =>
			await _context.Administradores.AsNoTracking()
				.FirstOrDefaultAsync(a => EF.Functions.ILike(a.Usuario, usuario));

		public async Task AgregarAsync(Administrador administrador)
		{
			_context.Administradores.Add(administrador);
			await _context.SaveChangesAsync();
		}

		public async Task<bool> ActualizarAsync(Administrador administrador)
		{
			if (!await _context.Administradores.AnyAsync(a => a.Id == administrador.Id)) return false;

			_context.Administradores.Update(administrador);
			await _context.SaveChangesAsync();
			return true;
		}

		public async Task<bool> EliminarAsync(int id)
		{
			var administrador = await _context.Administradores.FindAsync(id);
			if (administrador is null) return false;

			_context.Administradores.Remove(administrador);
			await _context.SaveChangesAsync();
			return true;
		}
	}
}
