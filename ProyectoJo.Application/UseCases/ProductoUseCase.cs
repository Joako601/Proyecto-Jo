using ProyectoJo.Application.Ports.In;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Application.UseCases
{
	public class ProductoUseCase : IProductoService
	{
		private readonly IProductoRepository _repository;

		public ProductoUseCase(IProductoRepository repository)
		{
			_repository = repository;
		}

		public IEnumerable<Item> ObtenerTodos()
		{
			return _repository.ObtenerTodos();
		}

		public IEnumerable<Item> ObtenerPorCategoria(string categoria)
		{
			return _repository.ObtenerPorCategoria(categoria);
		}

		public List<Item> ObtenerMenu() =>
			_repository.ObtenerMenu().Where(i => i.Activo).ToList();

		public void GuardarMenu(List<Item> menu) => _repository.GuardarMenu(menu);

		public void AgregarItem(Item item)
		{
			var menu = _repository.ObtenerMenu();
			item.Id = menu.Count > 0 ? menu.Max(i => i.Id) + 1 : 1;
			_repository.AgregarItem(item);
		}
		public Item? ObtenerPorId(int id) => _repository.ObtenerPorId(id);

		public void Eliminar(int id) => _repository.Eliminar(id);

		public void EditarItem(Item item)
		{
			var menu = _repository.ObtenerMenu();
			var index = menu.FindIndex(i => i.Id == item.Id);
			if (index >= 0) menu[index] = item;
			_repository.GuardarMenu(menu);
		}
		public void ToggleActivo(int id) => _repository.ToggleActivo(id);
		public void ToggleAgotado(int id) => _repository.ToggleAgotado(id);
	}
}