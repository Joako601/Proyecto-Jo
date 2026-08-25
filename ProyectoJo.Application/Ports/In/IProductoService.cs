using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Application.Ports.In
{
	public interface IProductoService
	{
		IEnumerable<Item> ObtenerTodos();
		List<Item> ObtenerMenu();
		void AgregarItem(Item item, string usuario);
		Item? ObtenerPorId(int id);
		bool Eliminar(int id, string usuario);
		bool EditarItem(Item item, string usuario);
		bool ToggleActivo(int id, string usuario);
		bool ToggleAgotado(int id, string usuario);
	}
}