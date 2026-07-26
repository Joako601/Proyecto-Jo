using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Application.DTOs
{
	public class ResultadoCrearPedido
	{
		public Pedido Pedido { get; set; } = null!;
		public List<LineaDescartada> LineasDescartadas { get; set; } = new();
		public List<LineaAjustada> LineasAjustadas { get; set; } = new();
	}

	public class LineaDescartada
	{
		public int ItemId { get; set; }
		public string Nombre { get; set; } = "";
		public string Motivo { get; set; } = "";
	}

	public class LineaAjustada
	{
		public int ItemId { get; set; }
		public string Nombre { get; set; } = "";
		public int CantidadSolicitada { get; set; }
		public int CantidadFinal { get; set; }
	}
}