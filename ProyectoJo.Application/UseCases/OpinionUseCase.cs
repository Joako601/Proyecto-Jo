using ProyectoJo.Application.DTOs;
using ProyectoJo.Application.Ports.In;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Application.UseCases
{
	public class OpinionUseCase : IOpinionService
	{
		private readonly IOpinionRepository _repository;
		private readonly IProductoService _productoService;
		private readonly IAuditoriaService _auditoriaService;

		public OpinionUseCase(
			IOpinionRepository repository,
			IProductoService productoService,
			IAuditoriaService auditoriaService)
		{
			_repository = repository;
			_productoService = productoService;
			_auditoriaService = auditoriaService;
		}

		public List<OpinionDto> ObtenerTodas()
		{
			return _repository.ObtenerTodas()
				.OrderByDescending(o => o.Fecha)
				.Select(ArmarDto)
				.ToList();
		}

		public OpinionCliente? ObtenerPorId(int id) => _repository.ObtenerPorId(id);

		public void Agregar(OpinionCliente opinion, string usuario)
		{
			opinion.Fecha = DateTime.Now;
			opinion.RegistradoPor = usuario;
			_repository.Agregar(opinion);

			_auditoriaService.RegistrarAccion(
				usuario: usuario,
				modulo: "Semáforo Feedback",
				accion: TipoAccionAuditoria.Creacion,
				entidad: $"Opinión #{opinion.Id} - {DescribirDestino(opinion.ItemId)}",
				detalleDespues: $"{opinion.Calificacion.ToString("0.0")} estrellas - {opinion.Estado}"
			);
		}

		public bool Editar(OpinionCliente opinion, string usuario)
		{
			var anterior = _repository.ObtenerPorId(opinion.Id);
			if (anterior is null) return false;

			opinion.Fecha = anterior.Fecha;
			opinion.RegistradoPor = anterior.RegistradoPor;

			var actualizado = _repository.Editar(opinion);
			if (!actualizado) return false;

			_auditoriaService.RegistrarAccion(
				usuario: usuario,
				modulo: "Semáforo Feedback",
				accion: TipoAccionAuditoria.Edicion,
				entidad: $"Opinión #{opinion.Id} - {DescribirDestino(opinion.ItemId)}",
				detalleAntes: $"{anterior.Calificacion.ToString("0.0")} estrellas - {anterior.Estado}",
				detalleDespues: $"{opinion.Calificacion.ToString("0.0")} estrellas - {opinion.Estado}"
			);

			return true;
		}

		public bool Eliminar(int id, string usuario)
		{
			var opinion = _repository.ObtenerPorId(id);
			if (opinion is null) return false;

			var eliminado = _repository.Eliminar(id);
			if (!eliminado) return false;

			_auditoriaService.RegistrarAccion(
				usuario: usuario,
				modulo: "Semáforo Feedback",
				accion: TipoAccionAuditoria.Eliminacion,
				entidad: $"Opinión #{id} - {DescribirDestino(opinion.ItemId)}",
				detalleAntes: $"{opinion.Calificacion.ToString("0.0")} estrellas - {opinion.Estado}"
			);

			return true;
		}

		public int ContarTotal() => _repository.ObtenerTodas().Count;

		public int ContarPorEstado(EstadoSemaforo estado) =>
			_repository.ObtenerTodas().Count(o => o.Estado == estado);

		private string DescribirDestino(int? itemId)
		{
			if (itemId is null) return "Opinión general";
			var item = _productoService.ObtenerPorId(itemId.Value);
			return item?.Platillo ?? "Platillo eliminado";
		}

		private OpinionDto ArmarDto(OpinionCliente opinion)
		{
			var item = opinion.ItemId is not null ? _productoService.ObtenerPorId(opinion.ItemId.Value) : null;

			return new OpinionDto
			{
				Id = opinion.Id,
				ItemId = opinion.ItemId,
				Platillo = opinion.ItemId is null ? null : (item?.Platillo ?? "Platillo eliminado"),
				NombreCliente = opinion.NombreCliente,
				Comentario = opinion.Comentario,
				Calificacion = opinion.Calificacion,
				Estado = opinion.Estado,
				Fecha = opinion.Fecha
			};
		}
	}
}