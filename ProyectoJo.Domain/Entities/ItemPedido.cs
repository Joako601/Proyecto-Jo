namespace ProyectoJo.Domain.Entities
{
	public class ItemPedido
	{
		public int ItemId { get; set; }
		public string Nombre { get; set; } = string.Empty;
		public int Cantidad { get; set; }
		public decimal PrecioUnitario { get; set; }
		public decimal Subtotal => Cantidad * PrecioUnitario;
	}
}