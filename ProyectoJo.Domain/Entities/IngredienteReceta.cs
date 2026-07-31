using System.ComponentModel.DataAnnotations;

namespace ProyectoJo.Domain.Entities
{
	public enum UnidadIngrediente
	{
		Kilogramo,
		Gramo,
		Litro,
		Mililitro,
		Unidad
	}

	public class IngredienteReceta
	{
		public int InsumoId { get; set; }
		public string Nombre { get; set; } = string.Empty;

		[Range(0.01, double.MaxValue, ErrorMessage = "La cantidad del ingrediente debe ser mayor a 0.")]
		public decimal Cantidad { get; set; }
		public UnidadIngrediente Unidad { get; set; }

		[Range(0, double.MaxValue, ErrorMessage = "El costo unitario no puede ser negativo.")]
		public decimal CostoUnitario { get; set; }

		public decimal CostoTotal => Math.Round(Cantidad * CostoUnitario, 2);
	}
}