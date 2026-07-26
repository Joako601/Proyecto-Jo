namespace ProyectoJo.Domain.Entities
{
	public enum EstadoSemaforo
	{
		Verde,
		Amarillo,
		Rojo
	}

	public class OpinionCliente
	{
		public int Id { get; set; }
		public int? ItemId { get; set; }
		public string? NombreCliente { get; set; }
		public string Comentario { get; set; } = string.Empty;
		public decimal Calificacion { get; set; }
		public EstadoSemaforo Estado { get; set; }
		public DateTime Fecha { get; set; } = DateTime.Now;
		public string RegistradoPor { get; set; } = string.Empty;
	}
}