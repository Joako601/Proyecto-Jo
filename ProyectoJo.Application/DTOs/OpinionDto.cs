using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Application.DTOs
{
	public class OpinionDto
	{
		public int Id { get; set; }
		public int? ItemId { get; set; }

		public string? Platillo { get; set; }

		public string? NombreCliente { get; set; }
		public string Comentario { get; set; } = string.Empty;
		public decimal Calificacion { get; set; }
		public EstadoSemaforo Estado { get; set; }
		public DateTime Fecha { get; set; }
	}
}