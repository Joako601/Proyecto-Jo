using System.Text.Json;
using System.Text.Json.Serialization;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Infrastructure.Persistence
{
	public class JsonPedidoRepository : IPedidoRepository
	{
		private readonly string _filePath;

		private static readonly SemaphoreSlim _lock = new(1, 1);

		private readonly JsonSerializerOptions _options = new()
		{
			WriteIndented = true,
			Converters = { new JsonStringEnumConverter() }
		};

		public JsonPedidoRepository(string filePath)
		{
			_filePath = filePath;
		}

		public async Task<List<Pedido>> ObtenerTodosAsync()
		{
			await _lock.WaitAsync();
			try
			{
				return await LeerAsync();
			}
			finally
			{
				_lock.Release();
			}
		}

		private async Task<List<Pedido>> LeerAsync()
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
			await _lock.WaitAsync();
			try
			{
				var todos = await LeerAsync();
				pedido.Id = todos.Any() ? todos.Max(p => p.Id) + 1 : 1;
				todos.Add(pedido);
				await EscribirAtomicoAsync(todos);
				return pedido;
			}
			finally
			{
				_lock.Release();
			}
		}

		public async Task<Pedido?> ActualizarAsync(Pedido pedido)
		{
			await _lock.WaitAsync();
			try
			{
				var todos = await LeerAsync();
				var index = todos.FindIndex(p => p.Id == pedido.Id);
				if (index == -1) return null;
				todos[index] = pedido;
				await EscribirAtomicoAsync(todos);
				return pedido;
			}
			finally
			{
				_lock.Release();
			}
		}

		public async Task<(Pedido? Anterior, Pedido? Actualizado, string? MotivoRechazo)> CambiarEstadoAtomicoAsync(
	int id,
	EstadoPedido nuevoEstado,
	Func<Pedido, Task<string?>>? validarAntesDeAplicar = null)
		{
			await _lock.WaitAsync();
			try
			{
				var todos = await LeerAsync();
				var index = todos.FindIndex(p => p.Id == id);
				if (index == -1) return (null, null, null);

				var anterior = todos[index];
				var estadoAnterior = anterior.Estado;

				var anteriorSnapshot = new Pedido
				{
					Id = anterior.Id,
					Estado = estadoAnterior,
					Mesa = anterior.Mesa,
					FechaCreacion = anterior.FechaCreacion,
					Items = anterior.Items
				};

				if (validarAntesDeAplicar is not null)
				{
					var motivoRechazo = await validarAntesDeAplicar(anteriorSnapshot);
					if (motivoRechazo is not null)
						return (anteriorSnapshot, null, motivoRechazo);
				}

				anterior.Estado = nuevoEstado;
				await EscribirAtomicoAsync(todos);

				return (anteriorSnapshot, anterior, null);
			}
			finally
			{
				_lock.Release();
			}
		}

		private async Task EscribirAtomicoAsync(List<Pedido> todos)
		{
			var json = JsonSerializer.Serialize(todos, _options);
			var rutaTemporal = _filePath + ".tmp";
			await File.WriteAllTextAsync(rutaTemporal, json);
			File.Move(rutaTemporal, _filePath, overwrite: true);
		}
	}
}