using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Application.DTOs
{
	public class ResultadoCambiarEstado
	{
		public Pedido? Pedido { get; set; }
		public bool Exitoso { get; set; }
		public string? MotivoRechazo { get; set; }
		public bool NoEncontrado { get; set; }
	}
}