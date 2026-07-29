using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Application.Ports.In
{
	public interface IEmpleadoAuthService
	{
		Task<Empleado?> ValidarCredencialesAsync(string nombre, string clave, RolEmpleado estacion);
	}
}