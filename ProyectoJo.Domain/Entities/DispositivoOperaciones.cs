namespace ProyectoJo.Domain.Entities
{
	public class DispositivoOperaciones
	{
		public string Token { get; set; } = string.Empty;
		public RolEmpleado Estacion { get; set; }
		public string Nombre { get; set; } = string.Empty;
		public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
	}
}