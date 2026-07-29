using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Application.Ports.Out
{
	public interface IAdministradorRepository
	{
		Task<List<Administrador>> ObtenerTodosAsync();
		Task<Administrador?> ObtenerPorIdAsync(int id);
		Task<Administrador?> ObtenerPorUsuarioAsync(string usuario);
		Task AgregarAsync(Administrador administrador);
		Task<bool> ActualizarAsync(Administrador administrador);
		Task<bool> EliminarAsync(int id);
	}
}