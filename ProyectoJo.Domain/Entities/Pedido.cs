using System.ComponentModel.DataAnnotations;

namespace ProyectoJo.Domain.Entities
{
	public class Pedido
	{
		public int Id { get; set; }

		[Required(ErrorMessage = "La mesa es obligatoria.")]
		[StringLength(50, ErrorMessage = "La mesa no puede superar los 50 caracteres.")]
		public string Mesa { get; set; } = string.Empty;
		public List<ItemPedido> Items { get; set; } = new();
		public EstadoPedido Estado { get; set; } = EstadoPedido.Pendiente;
		public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
		public decimal Total => Items.Sum(i => i.Subtotal);
		public bool PuedeTransicionarA(EstadoPedido nuevoEstado)
		{
			return (Estado, nuevoEstado) switch
			{
				(EstadoPedido.Pendiente, EstadoPedido.Preparado) => true,
				(EstadoPedido.Pendiente, EstadoPedido.Pagado) => true,
				(EstadoPedido.Pendiente, EstadoPedido.Cancelado) => true,
				(EstadoPedido.Preparado, EstadoPedido.Pagado) => true,
				(EstadoPedido.Preparado, EstadoPedido.Cancelado) => true,
				_ => false
			};
		}
	}
}