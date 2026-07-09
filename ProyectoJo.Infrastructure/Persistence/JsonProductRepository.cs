using System.Text.Json;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Infrastructure.Persistence
{
	public class JsonProductRepository : IProductoRepository
	{
		private readonly string _rutaArchivo;
		private static readonly object _lock = new();
		private static readonly JsonSerializerOptions _opciones = new() { WriteIndented = false };

		private List<Item>? _cache;

		public JsonProductRepository(string rutaArchivo)
		{
			_rutaArchivo = rutaArchivo;
		}

		public IEnumerable<Item> ObtenerTodos()
		{
			lock (_lock) { return ObtenerCache(); }
		}

		public IEnumerable<Item> ObtenerPorCategoria(string categoria)
		{
			lock (_lock) { return ObtenerCache().Where(i => i.Categoria == categoria).ToList(); }
		}

		public List<Item> ObtenerMenu()
		{
			lock (_lock) { return ObtenerCache(); }
		}

		public void GuardarMenu(List<Item> menu)
		{
			lock (_lock) { PersistirSinCandado(menu); }
		}

		public void AgregarItem(Item item)
		{
			lock (_lock)
			{
				var menu = ObtenerCache();
				item.Id = menu.Count > 0 ? menu.Max(i => i.Id) + 1 : 1;
				menu.Add(item);
				PersistirSinCandado(menu);
			}
		}

		public Item? ObtenerPorId(int id)
		{
			lock (_lock) { return ObtenerCache().Find(i => i.Id == id); }
		}

		public void Eliminar(int id)
		{
			lock (_lock)
			{
				var menu = ObtenerCache();
				menu.RemoveAll(i => i.Id == id);
				PersistirSinCandado(menu);
			}
		}

		public void ToggleActivo(int id)
		{
			lock (_lock)
			{
				var menu = ObtenerCache();
				var item = menu.Find(i => i.Id == id);
				if (item != null) item.Activo = !item.Activo;
				PersistirSinCandado(menu);
			}
		}

		public void ToggleAgotado(int id)
		{
			lock (_lock)
			{
				var menu = ObtenerCache();
				var item = menu.Find(i => i.Id == id);
				if (item != null) item.Agotado = !item.Agotado;
				PersistirSinCandado(menu);
			}
		}

		
		private List<Item> ObtenerCache()
		{
			_cache ??= LeerDesdeDisco();
			return _cache;
		}

		private List<Item> LeerDesdeDisco()
		{
			if (!File.Exists(_rutaArchivo)) return new List<Item>();
			var json = File.ReadAllText(_rutaArchivo);
			return JsonSerializer.Deserialize<List<Item>>(json, _opciones) ?? new List<Item>();
		}

		private void PersistirSinCandado(List<Item> menu)
		{
			var json = JsonSerializer.Serialize(menu, _opciones);
			var rutaTemporal = _rutaArchivo + ".tmp";
			File.WriteAllText(rutaTemporal, json);
			File.Move(rutaTemporal, _rutaArchivo, overwrite: true);
			_cache = menu;
		}
	}
}