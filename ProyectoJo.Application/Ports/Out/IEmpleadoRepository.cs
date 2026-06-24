using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Application.Ports.Out
{
	public interface IEmpleadoRepository
	{
		Task<List<Empleado>> ObtenerTodosAsync();
		Task<Empleado?> ObtenerPorIdAsync(int id);
	}
}