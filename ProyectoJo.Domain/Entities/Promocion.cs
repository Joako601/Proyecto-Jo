namespace ProyectoJo.Domain.Entities
{
	public class Promocion
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
	}
}