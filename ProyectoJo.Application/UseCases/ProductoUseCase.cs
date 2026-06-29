using ProyectoJo.Application.Ports.In;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Application.UseCases
{
	public class ProductoUseCase : IProductoService
	{
		private readonly IProductoRepository _repository;
		private readonly IAuditoriaService _auditoriaService;

		public ProductoUseCase(IProductoRepository repository, IAuditoriaService auditoriaService)
		{
			_repository = repository;
			_auditoriaService = auditoriaService;
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

		public void AgregarItem(Item item, string usuario)
		{
			_repository.AgregarItem(item);

			_auditoriaService.RegistrarAccion(
				usuario: usuario,
				modulo: "Productos",
				accion: TipoAccionAuditoria.Creacion,
				entidad: $"Producto #{item.Id} - {item.Platillo}",
				detalleDespues: $"{item.Platillo} - ${item.Precio}"
			);
		}

		public Item? ObtenerPorId(int id) => _repository.ObtenerPorId(id);

		public void Eliminar(int id, string usuario)
		{
			var item = _repository.ObtenerPorId(id);
			_repository.Eliminar(id);

			_auditoriaService.RegistrarAccion(
				usuario: usuario,
				modulo: "Productos",
				accion: TipoAccionAuditoria.Eliminacion,
				entidad: $"Producto #{id}",
				detalleAntes: item is not null ? $"{item.Platillo} - ${item.Precio}" : null
			);
		}

		public void EditarItem(Item item, string usuario)
		{
			var menu = _repository.ObtenerMenu();
			var anterior = menu.FirstOrDefault(i => i.Id == item.Id);
			var index = menu.FindIndex(i => i.Id == item.Id);
			if (index >= 0) menu[index] = item;
			_repository.GuardarMenu(menu);

			_auditoriaService.RegistrarAccion(
				usuario: usuario,
				modulo: "Productos",
				accion: TipoAccionAuditoria.Edicion,
				entidad: $"Producto #{item.Id} - {item.Platillo}",
				detalleAntes: anterior is not null ? $"{anterior.Platillo} - ${anterior.Precio}" : null,
				detalleDespues: $"{item.Platillo} - ${item.Precio}"
			);
		}

		public void ToggleActivo(int id, string usuario)
		{
			_repository.ToggleActivo(id);

			_auditoriaService.RegistrarAccion(
				usuario: usuario,
				modulo: "Productos",
				accion: TipoAccionAuditoria.Edicion,
				entidad: $"Producto #{id}",
				detalleDespues: "Se alternó el estado Activo/Inactivo"
			);
		}

		public void ToggleAgotado(int id, string usuario)
		{
			_repository.ToggleAgotado(id);

			_auditoriaService.RegistrarAccion(
				usuario: usuario,
				modulo: "Productos",
				accion: TipoAccionAuditoria.Edicion,
				entidad: $"Producto #{id}",
				detalleDespues: "Se alternó el estado Agotado/Disponible"
			);
		}
	}
}