namespace ProyectoJo.Domain.Entities
{
	public enum TipoAccionAuditoria
	{
		Creacion,
		Edicion,
		Eliminacion
	}

	public class RegistroAuditoria
	{
		public int Id { get; set; }
		public DateTime FechaHora { get; set; }
		public string Usuario { get; set; } = string.Empty;
		public string Modulo { get; set; } = string.Empty;
		public TipoAccionAuditoria Accion { get; set; }
		public string Entidad { get; set; } = string.Empty;
		public string? DetalleAntes { get; set; }
		public string? DetalleDespues { get; set; }
	}
}
