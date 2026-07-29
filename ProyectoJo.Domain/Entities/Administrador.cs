namespace ProyectoJo.Domain.Entities
{
	public class Administrador
	{
		public int Id { get; set; }
		public string Usuario { get; set; } = string.Empty;
		public string ContrasenaHash { get; set; } = string.Empty;
		public bool Activo { get; set; } = true;

		public List<string> Areas { get; set; } = new();
	}
}