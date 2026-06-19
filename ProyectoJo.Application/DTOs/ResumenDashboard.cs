namespace ProyectoJo.Application.DTOs
{
	public class ResumenDashboard
	{
		public decimal TotalIngresosHistorico { get; set; }
		public decimal TotalEgresosHistorico { get; set; }
		public decimal SaldoNetoHistorico => TotalIngresosHistorico - TotalEgresosHistorico;
		public int TotalMovimientos { get; set; }

		
		public decimal VentasAnio { get; set; }
		public decimal VentasMes { get; set; }
		public decimal VentasDia { get; set; }

		
		public decimal VentasMesPasado { get; set; }
		public decimal TicketPromedioMesPasado { get; set; }

		
		public List<TendenciaMensual> TendenciaAnio { get; set; } = new();

		public List<TendenciaMensual> UltimosSeisMeses { get; set; } = new();
		public List<CategoriaResumen> TopCategorias { get; set; } = new();
		public List<CategoriaResumen> TopCategoriasIngresos { get; set; } = new();
	}

	public class TendenciaMensual
	{
		public int Mes { get; set; }
		public int Anio { get; set; }
		public string Etiqueta { get; set; } = "";
		public decimal Ingresos { get; set; }
		public decimal Egresos { get; set; }
		public decimal SaldoNeto => Ingresos - Egresos;
	}

	public class CategoriaResumen
	{
		public string Categoria { get; set; } = "";
		public decimal Total { get; set; }
		public int Cantidad { get; set; }
	}
}