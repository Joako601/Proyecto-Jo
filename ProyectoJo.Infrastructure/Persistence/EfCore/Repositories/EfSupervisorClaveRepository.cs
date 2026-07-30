using Microsoft.EntityFrameworkCore;
using ProyectoJo.Application.Ports.Out;

namespace ProyectoJo.Infrastructure.Persistence.EfCore.Repositories
{
	public class EfSupervisorClaveRepository : ISupervisorClaveRepository
	{
		private const int FilaUnicaId = 1;

		private readonly ProyectoJoDbContext _context;

		public EfSupervisorClaveRepository(ProyectoJoDbContext context)
		{
			_context = context;
		}

		public async Task<string?> ObtenerHashAsync()
		{
			var fila = await _context.SupervisorClave.AsNoTracking().FirstOrDefaultAsync(s => s.Id == FilaUnicaId);
			return string.IsNullOrWhiteSpace(fila?.ClaveHash) ? null : fila.ClaveHash;
		}

		public async Task GuardarHashAsync(string hash)
		{
			var fila = await _context.SupervisorClave.FirstOrDefaultAsync(s => s.Id == FilaUnicaId);
			if (fila is null)
			{
				_context.SupervisorClave.Add(new SupervisorClave { Id = FilaUnicaId, ClaveHash = hash });
			}
			else
			{
				fila.ClaveHash = hash;
			}

			await _context.SaveChangesAsync();
		}
	}
}
