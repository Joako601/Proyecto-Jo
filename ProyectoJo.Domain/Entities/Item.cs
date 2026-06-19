namespace ProyectoJo.Domain.Entities
{
	public class Item
	{
		public int Id { get; set; }
		public string Platillo { get; set; }
		public string Categoria { get; set; }
		public decimal Precio { get; set; }
		public string Ingredientes { get; set; }
		public string Descripcion { get; set; }
		public string Base { get; set; }
		public bool Activo { get; set; } = true;
		public bool Agotado { get; set; } = false;
	}
}