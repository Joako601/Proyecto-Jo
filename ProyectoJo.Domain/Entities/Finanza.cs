namespace ProyectoJo.Domain.Entities
{
	public class Finanza
	{
		public int Id { get; set; }
		public decimal Monto { get; set; }
		public TipoMovimiento Tipo { get; set; }
		public string Categoria { get; set; }
		public string Descripcion { get; set; }
		public DateTime Fecha { get; set; }
	}
}
