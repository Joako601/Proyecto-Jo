using Microsoft.EntityFrameworkCore;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Infrastructure.Persistence.EfCore.Repositories
{
	public class EfDispositivoRepository : IDispositivoRepository
	{
		private readonly ProyectoJoDbContext _context;

		public EfDispositivoRepository(ProyectoJoDbContext context)
		{
			_context = context;
		}

		public async Task<DispositivoOperaciones?> ObtenerPorTokenAsync(string token) =>
			await _context.Dispositivos.AsNoTracking().FirstOrDefaultAsync(d => d.Token == token);

		public async Task<DispositivoOperaciones> RegistrarAsync(DispositivoOperaciones dispositivo)
		{
			_context.Dispositivos.Add(dispositivo);
			await _context.SaveChangesAsync();
			return dispositivo;
		}

		public async Task<DispositivoOperaciones?> ActualizarEstacionAsync(string token, RolEmpleado estacion)
		{
			var dispositivo = await _context.Dispositivos.FirstOrDefaultAsync(d => d.Token == token);
			if (dispositivo is null) return null;

			dispositivo.Estacion = estacion;
			await _context.SaveChangesAsync();
			return dispositivo;
		}

		public async Task<List<DispositivoOperaciones>> ObtenerTodosAsync() =>
			await _context.Dispositivos.AsNoTracking().ToListAsync();
	}
}
