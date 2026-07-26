using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Application.Ports.Out
{
	public interface IRecetaRepository
	{
		List<Receta> ObtenerTodas();
		Receta? ObtenerPorId(int id);
		Receta? ObtenerPorItemId(int itemId);
		void Agregar(Receta receta);
		bool Editar(Receta receta);
		bool Eliminar(int id);
	}
}