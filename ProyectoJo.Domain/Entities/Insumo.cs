namespace ProyectoJo.Domain.Entities
{
	public class Insumo
	{
		public int Id { get; set; }
		public string Nombre { get; set; } = string.Empty;
		public UnidadIngrediente Unidad { get; set; }
		public decimal StockActual { get; set; }
		public decimal StockMinimo { get; set; }
		public bool Activo { get; set; } = true;

		public bool Agotado => StockActual <= 0;
		public bool StockBajo => !Agotado && StockActual <= StockMinimo;
	}
}