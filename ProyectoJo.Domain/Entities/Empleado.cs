namespace ProyectoJo.Domain.Entities
{
	public enum RolEmpleado
	{
		Cocina,
		Recepcion
	}

	public class Empleado
	{
		public int Id { get; set; }
		public string Nombre { get; set; } = string.Empty;
		public string PinHash { get; set; } = string.Empty;
		public RolEmpleado Rol { get; set; }
		public bool Activo { get; set; } = true;
	}
}