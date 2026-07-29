using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Application.Ports.Out
{
	public interface IDispositivoRepository
	{
		Task<DispositivoOperaciones?> ObtenerPorTokenAsync(string token);
		Task<DispositivoOperaciones> RegistrarAsync(DispositivoOperaciones dispositivo);
		Task<DispositivoOperaciones?> ActualizarEstacionAsync(string token, RolEmpleado estacion);
		Task<List<DispositivoOperaciones>> ObtenerTodosAsync();
	}
}