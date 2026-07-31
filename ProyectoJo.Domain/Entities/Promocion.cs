using System.ComponentModel.DataAnnotations;

namespace ProyectoJo.Domain.Entities
{
	public class Promocion : IValidatableObject
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
			if (TipoDescuento == TipoDescuento.Ninguno) yield break;

			if (ValorDescuento is null || ValorDescuento <= 0)
			{
				yield return new ValidationResult(
					"El valor del descuento debe ser mayor a 0.",
					new[] { nameof(ValorDescuento) });
				yield break;
			}

			if (TipoDescuento == TipoDescuento.Porcentaje && ValorDescuento > 100)
			{
				yield return new ValidationResult(
					"El descuento porcentual no puede superar el 100%.",
					new[] { nameof(ValorDescuento) });
			}
		}
	}
}