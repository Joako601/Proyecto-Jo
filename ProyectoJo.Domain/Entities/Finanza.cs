using System.ComponentModel.DataAnnotations;

namespace ProyectoJo.Domain.Entities
{
	public class Finanza : IEntidadConId
	{
		public int Id { get; set; }

		[Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0.")]
		public decimal Monto { get; set; }
		public TipoMovimiento Tipo { get; set; }
		public string Categoria { get; set; }
		public string Descripcion { get; set; }
		public DateTime Fecha { get; set; }
	}
}
