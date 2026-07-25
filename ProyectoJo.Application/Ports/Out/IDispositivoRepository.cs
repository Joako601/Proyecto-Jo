using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Application.Ports.Out
{
	public interface IDispositivoRepository
	{
		Task<DispositivoOperaciones?> ObtenerPorTokenAsync(string token);
		Task<DispositivoOperaciones> RegistrarAsync(DispositivoOperaciones dispositivo);
		Task<List<DispositivoOperaciones>> ObtenerTodosAsync();
	}
}