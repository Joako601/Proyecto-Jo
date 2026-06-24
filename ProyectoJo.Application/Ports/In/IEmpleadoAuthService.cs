using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Application.Ports.In
{
	public interface IEmpleadoAuthService
	{
		Task<Empleado?> ValidarPinAsync(string pin, RolEmpleado estacion);
	}
}