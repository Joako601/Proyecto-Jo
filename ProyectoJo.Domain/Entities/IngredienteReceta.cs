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
		public decimal Cantidad { get; set; }
		public UnidadIngrediente Unidad { get; set; }
		public decimal CostoUnitario { get; set; }

		public decimal CostoTotal => Math.Round(Cantidad * CostoUnitario, 2);
	}
}