namespace ProyectoJo.Application.DTOs
{
	public class ResumenFinanciero
	{
		public decimal TotalIngresos { get; set; }
		public decimal TotalEgresos { get; set; }
		public decimal SaldoNeto => TotalIngresos - TotalEgresos;
		public DateTime Desde { get; set; }
		public DateTime Hasta { get; set; }
	}
}
