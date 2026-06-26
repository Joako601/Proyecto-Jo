namespace ProyectoJo.Application.DTOs
{
	public class ResumenMapaCalor
	{
		public List<VentasPorHora> VentasPorHora { get; set; } = new();
		public List<ProductoMasVendido> TopProductos { get; set; } = new();
		public List<VentasPorDiaSemana> VentasPorDiaSemana { get; set; } = new();
		public List<VentasPorDia> HistorialPorDia { get; set; } = new();
		public DateTime FechaSeleccionada { get; set; }
		public int TotalPedidos { get; set; }
		public decimal TotalVendido { get; set; }

		public List<VentasPorMes> VentasPorMes { get; set; } = new();
		public int AnioMesesSeleccionado { get; set; }

		public List<VentasPorDia> DiasDelMesSeleccionado { get; set; } = new();
		public int? MesDetalleSeleccionado { get; set; }

		public DateTime InicioSemana { get; set; }
		public DateTime FinSemana { get; set; }
		public int SemanaOffset { get; set; }
		public bool SemanaHistoricoCompleto { get; set; }
	}

	public class VentasPorHora
	{
		public int Hora { get; set; }
		public string Etiqueta { get; set; } = "";
		public int CantidadPedidos { get; set; }
		public decimal TotalVendido { get; set; }
	}

	public class ProductoMasVendido
	{
		public string Nombre { get; set; } = "";
		public int CantidadVendida { get; set; }
		public decimal TotalGenerado { get; set; }
	}

	public class VentasPorDiaSemana
	{
		public DayOfWeek DiaSemana { get; set; }
		public string Etiqueta { get; set; } = "";
		public int CantidadPedidos { get; set; }
		public decimal TotalVendido { get; set; }
	}

	public class VentasPorDia
	{
		public DateTime Fecha { get; set; }
		public string Etiqueta { get; set; } = "";
		public int CantidadPedidos { get; set; }
		public decimal TotalVendido { get; set; }
	}

	public class VentasPorMes
	{
		public int Mes { get; set; }
		public string Etiqueta { get; set; } = "";      // "Ene", "Feb", "Mar"...
		public int CantidadPedidos { get; set; }
		public decimal TotalVendido { get; set; }
	}
}