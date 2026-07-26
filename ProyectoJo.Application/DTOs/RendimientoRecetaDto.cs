namespace ProyectoJo.Application.DTOs
{
	public class RendimientoRecetaDto
	{
		public int RecetaId { get; set; }
		public int ItemId { get; set; }
		public string Platillo { get; set; } = string.Empty;
		public decimal CostoTotal { get; set; }
		public decimal CostoPorPorcion { get; set; }
		public decimal PrecioVenta { get; set; }
		public decimal MargenPorPorcion => PrecioVenta - CostoPorPorcion;
		public decimal MargenPorcentual => PrecioVenta > 0
			? Math.Round((MargenPorPorcion / PrecioVenta) * 100, 1)
			: 0m;
	}
}