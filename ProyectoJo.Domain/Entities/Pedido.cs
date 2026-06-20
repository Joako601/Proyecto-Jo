namespace ProyectoJo.Domain.Entities
{
	public class Pedido
	{
		public int Id { get; set; }
		public string Mesa { get; set; } = string.Empty;
		public List<ItemPedido> Items { get; set; } = new();
		public EstadoPedido Estado { get; set; } = EstadoPedido.Pendiente;
		public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
		public decimal Total => Items.Sum(i => i.Subtotal);
	}
}