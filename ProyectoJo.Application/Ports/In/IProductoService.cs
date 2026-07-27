using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Application.Ports.In
{
	public interface IProductoService
	{
		IEnumerable<Item> ObtenerTodos();
		IEnumerable<Item> ObtenerPorCategoria(string categoria);
		List<Item> ObtenerMenu();
		void AgregarItem(Item item, string usuario);
		Item? ObtenerPorId(int id);
		bool Eliminar(int id, string usuario);
		bool EditarItem(Item item, string usuario);
		void ToggleActivo(int id, string usuario);
		void ToggleAgotado(int id, string usuario);
	}
}