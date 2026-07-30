using System.Linq;
using Moq;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Application.UseCases;
using ProyectoJo.Domain.Entities;
using Xunit;

namespace ProyectoJo.Application.Tests.UseCases
{
	public class ReporteUseCaseTests
	{
		private readonly Mock<IPedidoRepository> _repository = new();
		private readonly ReporteUseCase _useCase;

		public ReporteUseCaseTests()
		{
			_useCase = new ReporteUseCase(_repository.Object);
		}

		[Fact]
		public async Task ObtenerMapaCalorAsync_IgnoraPedidosNoPagados()
		{
			var fecha = new DateTime(2026, 3, 10, 14, 0, 0, DateTimeKind.Utc);
			var pedidos = new List<Pedido>
			{
				new()
				{
					Id = 1, Estado = EstadoPedido.Cancelado, FechaCreacion = fecha,
					Items = new List<ItemPedido> { new() { ItemId = 1, Nombre = "Tacos", Cantidad = 1, PrecioUnitario = 100 } }
				}
			};
			_repository.Setup(r => r.ObtenerTodosAsync()).ReturnsAsync(pedidos);

			var resumen = await _useCase.ObtenerMapaCalorAsync(desde: fecha);

			Assert.Equal(0, resumen.TotalPedidos);
			Assert.Equal(0m, resumen.TotalVendido);
			Assert.Empty(resumen.TopProductos);
		}

		[Fact]
		public async Task ObtenerMapaCalorAsync_AcumulaTotalesSoloDelDiaSeleccionado()
		{
			var fecha = new DateTime(2026, 3, 10, 14, 0, 0, DateTimeKind.Utc);
			var pedidos = new List<Pedido>
			{
				new()
				{
					Id = 1, Estado = EstadoPedido.Pagado, FechaCreacion = fecha,
					Items = new List<ItemPedido> { new() { ItemId = 1, Nombre = "Tacos", Cantidad = 2, PrecioUnitario = 50 } }
				},
				new()
				{
					Id = 2, Estado = EstadoPedido.Pagado, FechaCreacion = fecha.AddDays(-1),
					Items = new List<ItemPedido> { new() { ItemId = 1, Nombre = "Tacos", Cantidad = 1, PrecioUnitario = 50 } }
				}
			};
			_repository.Setup(r => r.ObtenerTodosAsync()).ReturnsAsync(pedidos);

			var resumen = await _useCase.ObtenerMapaCalorAsync(desde: fecha);

			Assert.Equal(1, resumen.TotalPedidos);
			Assert.Equal(100m, resumen.TotalVendido);
			Assert.Equal(fecha.Date, resumen.FechaSeleccionada);

			var horaConVentas = resumen.VentasPorHora.Single(v => v.Hora == 14);
			Assert.Equal(1, horaConVentas.CantidadPedidos);
			Assert.Equal(100m, horaConVentas.TotalVendido);
		}

		[Fact]
		public async Task ObtenerMapaCalorAsync_AcumulaTopProductosDeTodoElHistorico()
		{
			var pedidos = new List<Pedido>
			{
				new()
				{
					Id = 1, Estado = EstadoPedido.Pagado, FechaCreacion = new DateTime(2025, 1, 1),
					Items = new List<ItemPedido> { new() { ItemId = 1, Nombre = "Tacos", Cantidad = 3, PrecioUnitario = 10 } }
				},
				new()
				{
					Id = 2, Estado = EstadoPedido.Pagado, FechaCreacion = new DateTime(2026, 6, 1),
					Items = new List<ItemPedido> { new() { ItemId = 1, Nombre = "Tacos", Cantidad = 2, PrecioUnitario = 10 } }
				}
			};
			_repository.Setup(r => r.ObtenerTodosAsync()).ReturnsAsync(pedidos);

			var resumen = await _useCase.ObtenerMapaCalorAsync();

			var tacos = resumen.TopProductos.Single(p => p.Nombre == "Tacos");
			Assert.Equal(5, tacos.CantidadVendida);
			Assert.Equal(50m, tacos.TotalGenerado);
		}
	}
}
