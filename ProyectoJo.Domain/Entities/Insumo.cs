using System.ComponentModel.DataAnnotations;

namespace ProyectoJo.Domain.Entities
{
	public class Insumo : IEntidadConId
	{
		public int Id { get; set; }
		public string Nombre { get; set; } = string.Empty;
		public UnidadIngrediente Unidad { get; set; }

		[Range(0, double.MaxValue, ErrorMessage = "El stock actual no puede ser negativo.")]
		public decimal StockActual { get; set; }

		[Range(0, double.MaxValue, ErrorMessage = "El stock mínimo no puede ser negativo.")]
		public decimal StockMinimo { get; set; }
		public bool Activo { get; set; } = true;

		public bool Agotado => StockActual <= 0;
		public bool StockBajo => !Agotado && StockActual <= StockMinimo;
	}
}