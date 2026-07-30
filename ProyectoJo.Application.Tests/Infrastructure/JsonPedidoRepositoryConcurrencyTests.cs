using ProyectoJo.Domain.Entities;
using ProyectoJo.Infrastructure.Persistence;
using Xunit;

namespace ProyectoJo.Application.Tests.Infrastructure
{
	public class JsonPedidoRepositoryConcurrencyTests : IDisposable
	{
		private readonly string _rutaArchivo;
		private readonly JsonPedidoRepository _repository;

		public JsonPedidoRepositoryConcurrencyTests()
		{
			_rutaArchivo = Path.Combine(Path.GetTempPath(), $"pedidos_test_{Guid.NewGuid()}.json");
			_repository = new JsonPedidoRepository(_rutaArchivo);
		}

		public void Dispose()
		{
			if (File.Exists(_rutaArchivo)) File.Delete(_rutaArchivo);
		}

		[Fact]
		public async Task CambiarEstadoAtomicoAsync_ConCambiosConcurrentes_NoPierdeLosItemsDelPedido()
		{
			// Arrange: un pedido real con items, igual a como llegaría desde Recepción
			var pedidoOriginal = new Pedido
			{
				Mesa = "7",
				Items = new List<ItemPedido>
				{
					new() { ItemId = 1, Nombre = "Tacos", Cantidad = 3, PrecioUnitario = 50 },
					new() { ItemId = 2, Nombre = "Agua", Cantidad = 1, PrecioUnitario = 20 }
				},
				Estado = EstadoPedido.Pendiente
			};
			var guardado = await _repository.GuardarAsync(pedidoOriginal);

			// Act: Cocina y Recepción intentan cambiar el estado del MISMO pedido
			// casi al mismo tiempo, muchas veces.
			const int intentos = 30;
			var tareas = new List<Task>();
			for (int i = 0; i < intentos; i++)
			{
				var nuevoEstado = i % 2 == 0 ? EstadoPedido.Preparado : EstadoPedido.Pagado;
				tareas.Add(_repository.CambiarEstadoAtomicoAsync(guardado.Id, nuevoEstado));
			}

			await Task.WhenAll(tareas);

			var pedidoFinal = await _repository.ObtenerPorIdAsync(guardado.Id);

			Assert.NotNull(pedidoFinal);
			Assert.Equal("7", pedidoFinal!.Mesa);
			Assert.Equal(2, pedidoFinal.Items.Count);
			Assert.Equal(170, pedidoFinal.Total); // 3*50 + 1*20
			Assert.True(pedidoFinal.Estado == EstadoPedido.Preparado || pedidoFinal.Estado == EstadoPedido.Pagado);

			// solo debe existir un único pedido en el archivo 
			var todos = await _repository.ObtenerTodosAsync();
			Assert.Single(todos);
		}

		[Fact]
		public async Task CambiarEstadoAtomicoAsync_CuandoElPedidoNoExiste_DevuelveTuplaNula()
		{
			// Act
			var (anterior, actualizado, motivoRechazo) = await _repository.CambiarEstadoAtomicoAsync(999, EstadoPedido.Pagado);

			// Assert
			Assert.Null(anterior);
			Assert.Null(actualizado);
			Assert.Null(motivoRechazo);
		}
	}
}