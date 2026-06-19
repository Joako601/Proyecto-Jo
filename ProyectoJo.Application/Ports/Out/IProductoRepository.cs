using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Application.Ports.Out
{
	public interface IProductoRepository
	{
		IEnumerable<Item> ObtenerTodos();
		IEnumerable<Item> ObtenerPorCategoria(string categoria);
		List<Item> ObtenerMenu();
		void GuardarMenu(List<Item> menu);
		void AgregarItem(Item item);
		Item? ObtenerPorId(int id);
		void Eliminar(int id);
	}
}