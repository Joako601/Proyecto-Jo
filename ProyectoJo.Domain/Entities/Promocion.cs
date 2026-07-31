using System.ComponentModel.DataAnnotations;

namespace ProyectoJo.Domain.Entities
{
	public class Promocion : IValidatableObject, IEntidadConId
	{
		public int Id { get; set; }
		public string Titulo { get; set; } = string.Empty;
		public string? Descripcion { get; set; }
		public string? ImagenUrl { get; set; }
		public TipoDescuento TipoDescuento { get; set; } = TipoDescuento.Ninguno;
		public decimal? ValorDescuento { get; set; }

		public List<int> ItemIds { get; set; } = new();

		public bool Activa { get; set; } = true;
		public DateTime? FechaInicio { get; set; }
		public DateTime? FechaFin { get; set; }

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if (TipoDescuento != TipoDescuento.Ninguno)
			{
				if (ValorDescuento is null || ValorDescuento <= 0)
				{
					yield return new ValidationResult(
						"El valor del descuento debe ser mayor a 0.",
						new[] { nameof(ValorDescuento) });
				}
				else if (TipoDescuento == TipoDescuento.Porcentaje && ValorDescuento > 100)
				{
					yield return new ValidationResult(
						"El descuento porcentual no puede superar el 100%.",
						new[] { nameof(ValorDescuento) });
				}
			}

			if (!RangoDeFechasEsValido(FechaInicio, FechaFin))
			{
				yield return new ValidationResult(
					"La fecha de inicio no puede ser posterior a la fecha de fin.",
					new[] { nameof(FechaFin) });
			}
		}

		public static bool RangoDeFechasEsValido(DateTime? fechaInicio, DateTime? fechaFin) =>
			!fechaInicio.HasValue || !fechaFin.HasValue || fechaInicio.Value.Date <= fechaFin.Value.Date;
	}
}