using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Application.Ports.In
{
	public interface IDispositivoService
	{
		Task<DispositivoOperaciones> EmparejarAsync(RolEmpleado estacion, string nombre);
		Task<DispositivoOperaciones?> ReasignarEstacionAsync(string token, RolEmpleado estacion, string? nombre);
		Task<DispositivoOperaciones?> ReconocerAsync(string token);
		Task<List<DispositivoOperaciones>> ObtenerTodosAsync();
		Task<DispositivoOperaciones?> ToggleBloqueadoAsync(int id);
		Task<DispositivoOperaciones?> ToggleActivoAsync(int id);
	}
}