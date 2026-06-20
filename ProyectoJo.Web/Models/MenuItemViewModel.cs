using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Web.Models
{
	public class MenuItemViewModel
	{
		public Item Platillo { get; set; } = null!;
		public decimal PrecioFinal { get; set; }
		public bool TienePromocion => PrecioFinal < Platillo.Precio;
	}
}