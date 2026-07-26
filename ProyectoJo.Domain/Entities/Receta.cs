namespace ProyectoJo.Domain.Entities
{
	public class Receta
	{
		public int Id { get; set; }
		public int ItemId { get; set; }
		public string NombreReceta { get; set; } = string.Empty;
		public int Rendimiento { get; set; } = 1;

		public string UnidadRendimiento { get; set; } = "porciones";

		public List<IngredienteReceta> Ingredientes { get; set; } = new();
		public string? Notas { get; set; }
		public DateTime FechaActualizacion { get; set; } = DateTime.Now;

		public decimal CostoTotal => Ingredientes.Sum(i => i.CostoTotal);

		public decimal CostoPorPorcion => Rendimiento > 0
			? Math.Round(CostoTotal / Rendimiento, 2)
			: 0m;
	}
}