using System.ComponentModel.DataAnnotations;

namespace ProyectoJo.Domain.Entities
{
	public class Item
	{
		public int Id { get; set; }

		[Required(ErrorMessage = "El nombre del platillo es obligatorio.")]
		public string Platillo { get; set; }

		[Required(ErrorMessage = "La categoría es obligatoria.")]
		public string Categoria { get; set; }

		[Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0.")]
		public decimal Precio { get; set; }
		public string Ingredientes { get; set; }
		public string Descripcion { get; set; }
		public string Base { get; set; }
		public bool Activo { get; set; } = true;
		public bool Agotado { get; set; } = false;
		public string? ImagenUrl { get; set; }
	}
}