using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Application.Ports.In
{
	public interface IAdministradorService
	{
		Task<List<Administrador>> ObtenerTodosAsync();
		Task<Administrador?> ObtenerPorIdAsync(int id);
		Task<(bool Exito, string? Error)> CrearAsync(string usuario, string contrasena, List<string> areas);
		Task<(bool Exito, string? Error)> EditarAsync(int id, string usuario, bool activo, string? nuevaContrasena, List<string> areas);
		Task<bool> EliminarAsync(int id);
	}
}