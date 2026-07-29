using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Application.Ports.Out
{
	public interface IEmpleadoRepository
	{
		Task<List<Empleado>> ObtenerTodosAsync();
		Task<Empleado?> ObtenerPorIdAsync(int id);
		Task AgregarAsync(Empleado empleado);
		Task<bool> ActualizarAsync(Empleado empleado);
		Task<bool> EliminarAsync(int id);
	}
}