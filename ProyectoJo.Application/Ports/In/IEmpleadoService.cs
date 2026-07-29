using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Application.Ports.In
{
	public interface IEmpleadoService
	{
		Task<List<Empleado>> ObtenerTodosAsync();
		Task<Empleado?> ObtenerPorIdAsync(int id);
		Task<(bool Exito, string? Error)> CrearAsync(string nombre, string pin, RolEmpleado rol);
		Task<(bool Exito, string? Error)> EditarAsync(int id, string nombre, bool activo, RolEmpleado rol, string? nuevoPin);
		Task<bool> EliminarAsync(int id);
	}
}