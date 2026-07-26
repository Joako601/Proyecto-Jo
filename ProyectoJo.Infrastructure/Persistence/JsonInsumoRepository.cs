using System.Text.Json;
using System.Text.Json.Serialization;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Infrastructure.Persistence
{
	public class JsonInsumoRepository : IInsumoRepository
	{
		private readonly string _filePath;
		private static readonly SemaphoreSlim _lock = new(1, 1);

		private readonly JsonSerializerOptions _options = new()
		{
			WriteIndented = true,
			Converters = { new JsonStringEnumConverter() }
		};

		public JsonInsumoRepository(string filePath)
		{
			_filePath = filePath;
		}

		public List<Insumo> ObtenerTodos()
		{
			_lock.Wait();
			try { return Leer(); }
			finally { _lock.Release(); }
		}

		public Insumo? ObtenerPorId(int id) => ObtenerTodos().FirstOrDefault(i => i.Id == id);

		public void Agregar(Insumo insumo)
		{
			_lock.Wait();
			try
			{
				var todos = Leer();
				insumo.Id = todos.Any() ? todos.Max(i => i.Id) + 1 : 1;
				todos.Add(insumo);
				EscribirAtomico(todos);
			}
			finally { _lock.Release(); }
		}

		public bool Editar(Insumo insumo)
		{
			_lock.Wait();
			try
			{
				var todos = Leer();
				var index = todos.FindIndex(i => i.Id == insumo.Id);
				if (index == -1) return false;
				todos[index] = insumo;
				EscribirAtomico(todos);
				return true;
			}
			finally { _lock.Release(); }
		}

		public bool Eliminar(int id)
		{
			_lock.Wait();
			try
			{
				var todos = Leer();
				var eliminados = todos.RemoveAll(i => i.Id == id);
				if (eliminados == 0) return false;
				EscribirAtomico(todos);
				return true;
			}
			finally { _lock.Release(); }
		}

		public async Task<(bool Exitoso, List<FaltanteInsumo> Faltantes)> DescontarAtomicoAsync(
			Dictionary<int, decimal> consumoPorInsumoId)
		{
			await _lock.WaitAsync();
			try
			{
				var todos = Leer();
				var faltantes = new List<FaltanteInsumo>();

				foreach (var (insumoId, necesario) in consumoPorInsumoId)
				{
					var insumo = todos.FirstOrDefault(i => i.Id == insumoId);
					if (insumo is null || insumo.StockActual < necesario)
					{
						faltantes.Add(new FaltanteInsumo
						{
							InsumoId = insumoId,
							Nombre = insumo?.Nombre ?? $"Insumo #{insumoId}",
							Necesario = necesario,
							Disponible = insumo?.StockActual ?? 0
						});
					}
				}

				if (faltantes.Count > 0) return (false, faltantes);

				foreach (var (insumoId, necesario) in consumoPorInsumoId)
				{
					var insumo = todos.First(i => i.Id == insumoId);
					insumo.StockActual -= necesario;
				}

				await EscribirAtomicoAsync(todos);
				return (true, new List<FaltanteInsumo>());
			}
			finally { _lock.Release(); }
		}

		public async Task<Insumo?> ReponerAtomicoAsync(int id, decimal cantidad)
		{
			await _lock.WaitAsync();
			try
			{
				var todos = Leer();
				var insumo = todos.FirstOrDefault(i => i.Id == id);
				if (insumo is null) return null;

				insumo.StockActual += cantidad;
				await EscribirAtomicoAsync(todos);
				return insumo;
			}
			finally { _lock.Release(); }
		}

		private List<Insumo> Leer()
		{
			if (!File.Exists(_filePath)) return new List<Insumo>();
			var json = File.ReadAllText(_filePath);
			return JsonSerializer.Deserialize<List<Insumo>>(json, _options) ?? new List<Insumo>();
		}

		private void EscribirAtomico(List<Insumo> todos)
		{
			var json = JsonSerializer.Serialize(todos, _options);
			var rutaTemporal = _filePath + ".tmp";
			File.WriteAllText(rutaTemporal, json);
			File.Move(rutaTemporal, _filePath, overwrite: true);
		}

		private async Task EscribirAtomicoAsync(List<Insumo> todos)
		{
			var json = JsonSerializer.Serialize(todos, _options);
			var rutaTemporal = _filePath + ".tmp";
			await File.WriteAllTextAsync(rutaTemporal, json);
			File.Move(rutaTemporal, _filePath, overwrite: true);
		}
	}
}