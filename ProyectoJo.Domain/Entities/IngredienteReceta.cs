namespace ProyectoJo.Domain.Entities
{
	public class IngredienteReceta
	{
		public string Nombre { get; set; } = string.Empty;
		public decimal Cantidad { get; set; }
		public string Unidad { get; set; } = string.Empty;
		public decimal CostoUnitario { get; set; }

		public decimal CostoTotal => Math.Round(Cantidad * CostoUnitario, 2);
	}
}