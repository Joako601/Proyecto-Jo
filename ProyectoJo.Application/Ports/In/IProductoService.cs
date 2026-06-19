using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Application.Ports.In
{
	public interface IProductoService
	{
		IEnumerable<Item> ObtenerTodos();
        IEnumerable<Item> ObtenerPorCategoria(string categoria);
        List<Item> ObtenerMenu();
        void GuardarMenu(List<Item> menu);
        void AgregarItem(Item item);
		Item? ObtenerPorId(int id);
		void Eliminar(int id);
		void EditarItem(Item item);
		void ToggleActivo(int id);
		void ToggleAgotado(int id);
	}
}