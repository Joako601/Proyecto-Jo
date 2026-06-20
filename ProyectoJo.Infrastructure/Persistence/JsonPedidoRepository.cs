using System.Text.Json;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Infrastructure.Persistence
{
	public class JsonPedidoRepository : IPedidoRepository
	{
		private readonly string _filePath;
		private readonly JsonSerializerOptions _options = new() { WriteIndented = true };

		public JsonPedidoRepository(string filePath)
		{
			_filePath = filePath;
		}

		public async Task<List<Pedido>> ObtenerTodosAsync()
		{
			if (!File.Exists(_filePath)) return new List<Pedido>();
			var json = await File.ReadAllTextAsync(_filePath);
			return JsonSerializer.Deserialize<List<Pedido>>(json, _options) ?? new List<Pedido>();
		}

		public async Task<Pedido?> ObtenerPorIdAsync(int id)
		{
			var todos = await ObtenerTodosAsync();
			return todos.FirstOrDefault(p => p.Id == id);
		}

		public async Task<Pedido> GuardarAsync(Pedido pedido)
		{
			var todos = await ObtenerTodosAsync();
			pedido.Id = todos.Any() ? todos.Max(p => p.Id) + 1 : 1;
			todos.Add(pedido);
			await File.WriteAllTextAsync(_filePath, JsonSerializer.Serialize(todos, _options));
			return pedido;
		}

		public async Task<Pedido?> ActualizarAsync(Pedido pedido)
		{
			var todos = await ObtenerTodosAsync();
			var index = todos.FindIndex(p => p.Id == pedido.Id);
			if (index == -1) return null;
			todos[index] = pedido;
			await File.WriteAllTextAsync(_filePath, JsonSerializer.Serialize(todos, _options));
			return pedido;
		}
	}
}