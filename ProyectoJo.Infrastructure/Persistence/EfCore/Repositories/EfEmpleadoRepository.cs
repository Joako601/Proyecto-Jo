using Microsoft.EntityFrameworkCore;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Infrastructure.Persistence.EfCore.Repositories
{
	public class EfEmpleadoRepository : IEmpleadoRepository
	{
		private readonly ProyectoJoDbContext _context;

		public EfEmpleadoRepository(ProyectoJoDbContext context)
		{
			_context = context;
		}

		public async Task<List<Empleado>> ObtenerTodosAsync() =>
			await _context.Empleados.AsNoTracking().ToListAsync();

		public async Task<Empleado?> ObtenerPorIdAsync(int id) =>
			await _context.Empleados.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);

		public async Task AgregarAsync(Empleado empleado)
		{
			_context.Empleados.Add(empleado);
			await _context.SaveChangesAsync();
		}

		public async Task<bool> ActualizarAsync(Empleado empleado)
		{
			if (!await _context.Empleados.AnyAsync(e => e.Id == empleado.Id)) return false;

			_context.Empleados.Update(empleado);
			await _context.SaveChangesAsync();
			return true;
		}

		public async Task<bool> EliminarAsync(int id)
		{
			var empleado = await _context.Empleados.FindAsync(id);
			if (empleado is null) return false;

			_context.Empleados.Remove(empleado);
			await _context.SaveChangesAsync();
			return true;
		}
	}
}
