using ProyectoJo.Application.Ports.In;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Application.UseCases
{
	public class PromocionUseCase : IPromocionService
	{
		private readonly IPromocionRepository _repository;
		private readonly IAuditoriaService _auditoriaService;

		public PromocionUseCase(IPromocionRepository repository, IAuditoriaService auditoriaService)
		{
			_repository = repository;
			_auditoriaService = auditoriaService;
		}

		public IEnumerable<Promocion> ObtenerTodas() => _repository.ObtenerTodas();

		public Promocion? ObtenerPorId(int id) => _repository.ObtenerPorId(id);

		public bool EstaVigente(Promocion promocion)
		{
			var hoy = DateTime.Today;

			if (!promocion.Activa) return false;
			if (promocion.FechaInicio.HasValue && hoy < promocion.FechaInicio.Value.Date) return false;
			if (promocion.FechaFin.HasValue && hoy > promocion.FechaFin.Value.Date) return false;

			return true;
		}

		public IEnumerable<Promocion> ObtenerVigentes() =>
			_repository.ObtenerTodas().Where(EstaVigente);

		public IEnumerable<Promocion> ObtenerVigentesGenerales() =>
			ObtenerVigentes().Where(p => p.ItemIds == null || p.ItemIds.Count == 0);

		public IEnumerable<Promocion> ObtenerVigentesPorItem(int itemId) =>
			ObtenerVigentes().Where(p => p.ItemIds != null && p.ItemIds.Contains(itemId));

		public decimal CalcularPrecioFinal(Item item)
		{
			var promo = ObtenerVigentesPorItem(item.Id)
				.Where(p => p.TipoDescuento != TipoDescuento.Ninguno && p.ValorDescuento.HasValue)
				.OrderByDescending(p => p.Id)
				.FirstOrDefault();

			if (promo == null) return item.Precio;

			decimal precioFinal = promo.TipoDescuento switch
			{
				TipoDescuento.Porcentaje => item.Precio - (item.Precio * (promo.ValorDescuento!.Value / 100m)),
				TipoDescuento.MontoFijo => item.Precio - promo.ValorDescuento!.Value,
				_ => item.Precio
			};

			return precioFinal < 0 ? 0 : Math.Round(precioFinal, 2);
		}

		public void Agregar(Promocion promocion, string usuario)
		{
			_repository.Agregar(promocion);

			_auditoriaService.RegistrarAccion(
				usuario: usuario,
				modulo: "Promociones",
				accion: TipoAccionAuditoria.Creacion,
				entidad: $"Promoción #{promocion.Id} - {promocion.Titulo}",
				detalleDespues: $"{promocion.Titulo} - {promocion.TipoDescuento} {promocion.ValorDescuento}"
			);
		}

		public void Editar(Promocion promocion, string usuario)
		{
			var anterior = _repository.ObtenerPorId(promocion.Id);
			_repository.Editar(promocion);

			_auditoriaService.RegistrarAccion(
				usuario: usuario,
				modulo: "Promociones",
				accion: TipoAccionAuditoria.Edicion,
				entidad: $"Promoción #{promocion.Id} - {promocion.Titulo}",
				detalleAntes: anterior is not null ? $"{anterior.Titulo} - {anterior.TipoDescuento} {anterior.ValorDescuento}" : null,
				detalleDespues: $"{promocion.Titulo} - {promocion.TipoDescuento} {promocion.ValorDescuento}"
			);
		}

		public void Eliminar(int id, string usuario)
		{
			var promocion = _repository.ObtenerPorId(id);
			_repository.Eliminar(id);

			_auditoriaService.RegistrarAccion(
				usuario: usuario,
				modulo: "Promociones",
				accion: TipoAccionAuditoria.Eliminacion,
				entidad: $"Promoción #{id}",
				detalleAntes: promocion is not null ? $"{promocion.Titulo}" : null
			);
		}

		public void ToggleActiva(int id, string usuario)
		{
			_repository.ToggleActiva(id);

			_auditoriaService.RegistrarAccion(
				usuario: usuario,
				modulo: "Promociones",
				accion: TipoAccionAuditoria.Edicion,
				entidad: $"Promoción #{id}",
				detalleDespues: "Se alternó el estado Activa/Inactiva"
			);
		}
	}
}