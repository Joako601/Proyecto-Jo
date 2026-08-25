using ProyectoJo.Application.DTOs;
using ProyectoJo.Application.Ports.In;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Application.UseCases
{
	public class RecetaUseCase : IRecetaService
	{
		private readonly IRecetaRepository _repository;
		private readonly IProductoService _productoService;
		private readonly IAuditoriaService _auditoriaService;

		public RecetaUseCase(
			IRecetaRepository repository,
			IProductoService productoService,
			IAuditoriaService auditoriaService)
		{
			_repository = repository;
			_productoService = productoService;
			_auditoriaService = auditoriaService;
		}

		public List<Receta> ObtenerTodas() => _repository.ObtenerTodas();

		public Receta? ObtenerPorId(int id) => _repository.ObtenerPorId(id);

		public Receta? ObtenerPorItemId(int itemId) => _repository.ObtenerPorItemId(itemId);

		public void Agregar(Receta receta, string usuario)
		{
			receta.FechaActualizacion = DateTime.Now;
			_repository.Agregar(receta);

			var item = _productoService.ObtenerPorId(receta.ItemId);
			_auditoriaService.RegistrarAccion(
				usuario: usuario,
				modulo: "Recetario",
				accion: TipoAccionAuditoria.Creacion,
				entidad: $"Receta #{receta.Id} - {item?.Platillo ?? receta.NombreReceta}",
				detalleDespues: $"Costo por porción: ${receta.CostoPorPorcion}"
			);
		}

		public bool Editar(Receta receta, string usuario)
		{
			var anterior = _repository.ObtenerPorId(receta.Id);
			if (anterior is null) return false;

			receta.FechaActualizacion = DateTime.Now;
			var actualizado = _repository.Editar(receta);
			if (!actualizado) return false;

			var item = _productoService.ObtenerPorId(receta.ItemId);
			_auditoriaService.RegistrarAccion(
				usuario: usuario,
				modulo: "Recetario",
				accion: TipoAccionAuditoria.Edicion,
				entidad: $"Receta #{receta.Id} - {item?.Platillo ?? receta.NombreReceta}",
				detalleAntes: $"Costo por porción: ${anterior.CostoPorPorcion}",
				detalleDespues: $"Costo por porción: ${receta.CostoPorPorcion}"
			);

			return true;
		}

		public bool Eliminar(int id, string usuario)
		{
			var receta = _repository.ObtenerPorId(id);
			if (receta is null) return false;

			var eliminado = _repository.Eliminar(id);
			if (!eliminado) return false;

			var item = _productoService.ObtenerPorId(receta.ItemId);
			_auditoriaService.RegistrarAccion(
				usuario: usuario,
				modulo: "Recetario",
				accion: TipoAccionAuditoria.Eliminacion,
				entidad: $"Receta #{id} - {item?.Platillo ?? receta.NombreReceta}",
				detalleAntes: $"Costo por porción: ${receta.CostoPorPorcion}"
			);

			return true;
		}

		public List<RendimientoRecetaDto> ObtenerRendimientoDeTodas()
		{
			var recetas = _repository.ObtenerTodas();
			var productosPorId = _productoService.ObtenerTodos().ToDictionary(i => i.Id);

			return recetas
				.Select(r => ArmarDto(r, productosPorId.GetValueOrDefault(r.ItemId)))
				.Where(dto => dto is not null)
				.Select(dto => dto!)
				.ToList();
		}

		private static RendimientoRecetaDto? ArmarDto(Receta receta, Item? item)
		{
			if (item is null) return null;

			return new RendimientoRecetaDto
			{
				RecetaId = receta.Id,
				ItemId = item.Id,
				Platillo = item.Platillo,
				CostoTotal = receta.CostoTotal,
				CostoPorPorcion = receta.CostoPorPorcion,
				PrecioVenta = item.Precio
			};
		}
	}
}