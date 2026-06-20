using ProyectoJo.Application.Ports.In;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Application.UseCases
{
	public class PromocionUseCase : IPromocionService
	{
		private readonly IPromocionRepository _repository;

		public PromocionUseCase(IPromocionRepository repository)
		{
			_repository = repository;
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

		public void Agregar(Promocion promocion)
		{
			var todas = _repository.ObtenerTodas().ToList();
			promocion.Id = todas.Count > 0 ? todas.Max(p => p.Id) + 1 : 1;
			_repository.Agregar(promocion);
		}

		public void Editar(Promocion promocion) => _repository.Editar(promocion);

		public void Eliminar(int id) => _repository.Eliminar(id);

		public void ToggleActiva(int id) => _repository.ToggleActiva(id);
	}
}