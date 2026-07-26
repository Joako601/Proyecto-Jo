using System.Text.Json;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Infrastructure.Persistence
{
	public class JsonRecetaRepository : IRecetaRepository
	{
		private readonly string _rutaArchivo;
		private static readonly object _lock = new();
		private static readonly JsonSerializerOptions _opciones = new() { WriteIndented = false };

		private List<Receta>? _cache;

		public JsonRecetaRepository(string rutaArchivo)
		{
			_rutaArchivo = rutaArchivo;
		}

		public List<Receta> ObtenerTodas()
		{
			lock (_lock) { return ObtenerCache().ToList(); }
		}

		public Receta? ObtenerPorId(int id)
		{
			lock (_lock) { return ObtenerCache().Find(r => r.Id == id); }
		}

		public Receta? ObtenerPorItemId(int itemId)
		{
			lock (_lock) { return ObtenerCache().Find(r => r.ItemId == itemId); }
		}

		public void Agregar(Receta receta)
		{
			lock (_lock)
			{
				var recetas = ObtenerCache();
				receta.Id = recetas.Count > 0 ? recetas.Max(r => r.Id) + 1 : 1;
				recetas.Add(receta);
				PersistirSinCandado(recetas);
			}
		}

		public bool Editar(Receta receta)
		{
			lock (_lock)
			{
				var recetas = ObtenerCache();
				var index = recetas.FindIndex(r => r.Id == receta.Id);
				if (index < 0) return false;

				recetas[index] = receta;
				PersistirSinCandado(recetas);
				return true;
			}
		}

		public bool Eliminar(int id)
		{
			lock (_lock)
			{
				var recetas = ObtenerCache();
				var eliminadas = recetas.RemoveAll(r => r.Id == id);
				if (eliminadas == 0) return false;

				PersistirSinCandado(recetas);
				return true;
			}
		}

		private List<Receta> ObtenerCache()
		{
			_cache ??= LeerDesdeDisco();
			return _cache;
		}

		private List<Receta> LeerDesdeDisco()
		{
			if (!File.Exists(_rutaArchivo)) return new List<Receta>();
			var json = File.ReadAllText(_rutaArchivo);
			return JsonSerializer.Deserialize<List<Receta>>(json, _opciones) ?? new List<Receta>();
		}

		private void PersistirSinCandado(List<Receta> recetas)
		{
			var json = JsonSerializer.Serialize(recetas, _opciones);
			var rutaTemporal = _rutaArchivo + ".tmp";
			File.WriteAllText(rutaTemporal, json);
			File.Move(rutaTemporal, _rutaArchivo, overwrite: true);
			_cache = recetas;
		}
	}
}