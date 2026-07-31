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

			return CalcularPrecioFinal(item, ObtenerVigentes().ToList());
		}

		public decimal CalcularPrecioFinal(Item item, List<Promocion> promosVigentes)
		{
			var promo = promosVigentes
				.Where(p => p.ItemIds != null && p.ItemIds.Contains(item.Id)
						 && p.TipoDescuento != TipoDescuento.Ninguno && p.ValorDescuento.HasValue)
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

		public bool Editar(Promocion promocion, string usuario)
		{
			var anterior = _repository.ObtenerPorId(promocion.Id);
			if (anterior is null) return false;

			var actualizado = _repository.Editar(promocion);
			if (!actualizado) return false;

			_auditoriaService.RegistrarAccion(
				usuario: usuario,
				modulo: "Promociones",
				accion: TipoAccionAuditoria.Edicion,
				entidad: $"Promoción #{promocion.Id} - {promocion.Titulo}",
				detalleAntes: $"{anterior.Titulo} - {anterior.TipoDescuento} {anterior.ValorDescuento}",
				detalleDespues: $"{promocion.Titulo} - {promocion.TipoDescuento} {promocion.ValorDescuento}"
			);

			return true;
		}

		public bool Eliminar(int id, string usuario)
		{
			var promocion = _repository.ObtenerPorId(id);
			if (promocion is null) return false;

			var eliminado = _repository.Eliminar(id);
			if (!eliminado) return false;

			_auditoriaService.RegistrarAccion(
				usuario: usuario,
				modulo: "Promociones",
				accion: TipoAccionAuditoria.Eliminacion,
				entidad: $"Promoción #{id}",
				detalleAntes: $"{promocion.Titulo}"
			);

			return true;
		}

		public bool ToggleActiva(int id, string usuario)
		{
			var cambiado = _repository.ToggleActiva(id);
			if (!cambiado) return false;

			_auditoriaService.RegistrarAccion(
				usuario: usuario,
				modulo: "Promociones",
				accion: TipoAccionAuditoria.Edicion,
				entidad: $"Promoción #{id}",
				detalleDespues: "Se alternó el estado Activa/Inactiva"
			);

			return true;
		}

		public bool ActualizarFecha(int id, DateTime? fechaInicio, DateTime? fechaFin, string usuario)
		{
			if (fechaInicio.HasValue && fechaFin.HasValue && fechaInicio.Value.Date > fechaFin.Value.Date)
				throw new InvalidOperationException("La fecha de inicio no puede ser posterior a la fecha de fin.");

			var promocion = _repository.ObtenerPorId(id);
			if (promocion is null) return false;

			var inicioAnterior = promocion.FechaInicio;
			var finAnterior = promocion.FechaFin;

			promocion.FechaInicio = fechaInicio;
			promocion.FechaFin = fechaFin;
			var actualizado = _repository.Editar(promocion);
			if (!actualizado) return false;

			_auditoriaService.RegistrarAccion(
				usuario: usuario,
				modulo: "Promociones",
				accion: TipoAccionAuditoria.Edicion,
				entidad: $"Promoción #{id} - {promocion.Titulo}",
				detalleAntes: $"Vigencia: {inicioAnterior?.ToString("dd/MM/yyyy") ?? "—"} al {finAnterior?.ToString("dd/MM/yyyy") ?? "—"}",
				detalleDespues: $"Vigencia: {fechaInicio?.ToString("dd/MM/yyyy") ?? "—"} al {fechaFin?.ToString("dd/MM/yyyy") ?? "—"}"
			);

			return true;
		}

		public bool HacerPermanente(int id, string usuario)
		{
			var promocion = _repository.ObtenerPorId(id);
			if (promocion is null) return false;

			promocion.FechaInicio = null;
			promocion.FechaFin = null;
			promocion.Activa = true;
			var actualizado = _repository.Editar(promocion);
			if (!actualizado) return false;

			_auditoriaService.RegistrarAccion(
				usuario: usuario,
				modulo: "Promociones",
				accion: TipoAccionAuditoria.Edicion,
				entidad: $"Promoción #{id} - {promocion.Titulo}",
				detalleDespues: "Se convirtió en promoción permanente (sin fechas de vigencia)"
			);

			return true;
		}
	}
}