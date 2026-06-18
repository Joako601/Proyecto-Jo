using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Application.Ports.In
{
	public interface IProductoService
	{
		IEnumerable<Item> ObtenerTodos();
		IEnumerable<Item> ObtenerPorCategoria(string categoria);
		List<Item> ObtenerMenu();
		void GuardarMenu(List<Item> menu);
	}
}