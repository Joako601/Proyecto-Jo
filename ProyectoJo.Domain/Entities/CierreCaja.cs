namespace ProyectoJo.Domain.Entities
{
	public class CierreCaja
	{
		public int Id { get; set; }

		public EstadoCaja Estado { get; set; } = EstadoCaja.Abierta;

		// --- Datos de apertura ---
		public DateTime FechaApertura { get; set; }
		public decimal FondoInicial { get; set; }
		public string? NotasApertura { get; set; }

		// --- Datos de cierre  ---
		public DateTime? FechaCierre { get; set; }
		public decimal VentasDelDia { get; set; }
		public decimal GastosDelDia { get; set; }
		public string? NotasCierre { get; set; }

		
		public decimal Total => FondoInicial + VentasDelDia - GastosDelDia;
	}
}