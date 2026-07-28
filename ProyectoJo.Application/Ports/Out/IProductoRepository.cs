using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Application.Ports.Out
{
	public interface IProductoRepository
	{
		IEnumerable<Item> ObtenerTodos();
		IEnumerable<Item> ObtenerPorCategoria(string categoria);
		List<Item> ObtenerMenu();
		void ActualizarItem(Item item);
		void AgregarItem(Item item);
		Item? ObtenerPorId(int id);
		bool Eliminar(int id);
		bool ToggleActivo(int id);
		bool ToggleAgotado(int id);
	}
}