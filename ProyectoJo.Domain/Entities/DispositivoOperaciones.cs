namespace ProyectoJo.Domain.Entities
{
	public class DispositivoOperaciones
	{
		public int Id { get; set; }
		public string Token { get; set; } = string.Empty;
		public RolEmpleado Estacion { get; set; }
		public string Nombre { get; set; } = string.Empty;
		public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
		public bool Bloqueado { get; set; }
		public bool Activo { get; set; } = true;
	}
}