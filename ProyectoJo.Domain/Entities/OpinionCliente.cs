using System.ComponentModel.DataAnnotations;

namespace ProyectoJo.Domain.Entities
{
	public enum EstadoSemaforo
	{
		Verde,
		Amarillo,
		Rojo
	}

	public class OpinionCliente : IValidatableObject
	{
		public int Id { get; set; }
		public int? ItemId { get; set; }

		[StringLength(200, ErrorMessage = "El nombre del cliente no puede superar los 200 caracteres.")]
		public string? NombreCliente { get; set; }

		[Required(ErrorMessage = "Escribí el comentario del cliente.")]
		[StringLength(1000, ErrorMessage = "El comentario no puede superar los 1000 caracteres.")]
		public string Comentario { get; set; } = string.Empty;

		[Range(1, 5, ErrorMessage = "La calificación debe estar entre 1 y 5 estrellas.")]
		public decimal Calificacion { get; set; }

		public EstadoSemaforo Estado { get; set; }
		public DateTime Fecha { get; set; } = DateTime.Now;
		public string RegistradoPor { get; set; } = string.Empty;

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if ((Calificacion * 2) % 1 != 0)
			{
				yield return new ValidationResult(
					"La calificación solo admite pasos de media estrella.",
					new[] { nameof(Calificacion) });
			}
		}
	}
}