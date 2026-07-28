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

		public bool Eliminar(int id, string usuario)
		{
			var item = _repository.ObtenerPorId(id);
			if (item is null) return false;

			var eliminado = _repository.Eliminar(id);
			if (!eliminado) return false;

			_auditoriaService.RegistrarAccion(
				usuario: usuario,
				modulo: "Productos",
				accion: TipoAccionAuditoria.Eliminacion,
				entidad: $"Producto #{id}",
				detalleAntes: $"{item.Platillo} - ${item.Precio}"
			);

			return true;
		}

		public bool EditarItem(Item item, string usuario)
		{
			var anterior = _repository.ObtenerPorId(item.Id);
			if (anterior is null) return false;

			_repository.ActualizarItem(item);

			_auditoriaService.RegistrarAccion(
				usuario: usuario,
				modulo: "Productos",
				accion: TipoAccionAuditoria.Edicion,
				entidad: $"Producto #{item.Id} - {item.Platillo}",
				detalleAntes: $"{anterior.Platillo} - ${anterior.Precio}",
				detalleDespues: $"{item.Platillo} - ${item.Precio}"
			);

			return true;
		}

		public bool ToggleActivo(int id, string usuario)
		{
			var cambiado = _repository.ToggleActivo(id);
			if (!cambiado) return false;

			_auditoriaService.RegistrarAccion(
				usuario: usuario,
				modulo: "Productos",
				accion: TipoAccionAuditoria.Edicion,
				entidad: $"Producto #{id}",
				detalleDespues: "Se alternó el estado Activo/Inactivo"
			);

			return true;
		}

		public bool ToggleAgotado(int id, string usuario)
		{
			var cambiado = _repository.ToggleAgotado(id);
			if (!cambiado) return false;

			_auditoriaService.RegistrarAccion(
				usuario: usuario,
				modulo: "Productos",
				accion: TipoAccionAuditoria.Edicion,
				entidad: $"Producto #{id}",
				detalleDespues: "Se alternó el estado Agotado/Disponible"
			);

			return true;
		}
	}
}