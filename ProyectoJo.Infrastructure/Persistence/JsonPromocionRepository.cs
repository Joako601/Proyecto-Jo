using System.Text.Json;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Infrastructure.Persistence
{
	public class JsonPromocionRepository : IPromocionRepository
	{
		private readonly string _rutaArchivo;
		private static readonly object _lock = new();
		private static readonly JsonSerializerOptions _opciones = new() { WriteIndented = false };

		private List<Promocion>? _cache;

		public JsonPromocionRepository(string rutaArchivo)
		{
			_rutaArchivo = rutaArchivo;
		}

		public IEnumerable<Promocion> ObtenerTodas()
		{
			lock (_lock) { return ObtenerCache(); }
		}

		public Promocion? ObtenerPorId(int id)
		{
			lock (_lock) { return ObtenerCache().Find(p => p.Id == id); }
		}

		public void Agregar(Promocion promocion)
		{
			lock (_lock)
			{
				var promociones = ObtenerCache();
				promocion.Id = promociones.Count > 0 ? promociones.Max(p => p.Id) + 1 : 1;
				promociones.Add(promocion);
				PersistirSinCandado(promociones);
			}
		}

		public bool Editar(Promocion promocion)
		{
			lock (_lock)
			{
				var promociones = ObtenerCache();
				var index = promociones.FindIndex(p => p.Id == promocion.Id);
				if (index < 0) return false;

				promociones[index] = promocion;
				PersistirSinCandado(promociones);
				return true;
			}
		}

		public bool Eliminar(int id)
		{
			lock (_lock)
			{
				var promociones = ObtenerCache();
				var eliminadas = promociones.RemoveAll(p => p.Id == id);
				if (eliminadas == 0) return false;

				PersistirSinCandado(promociones);
				return true;
			}
		}

		public bool ToggleActiva(int id)
		{
			lock (_lock)
			{
				var promociones = ObtenerCache();
				var promo = promociones.Find(p => p.Id == id);
				if (promo is null) return false;

				promo.Activa = !promo.Activa;
				PersistirSinCandado(promociones);
				return true;
			}
		}

		private List<Promocion> ObtenerCache()
		{
			_cache ??= LeerDesdeDisco();
			return _cache;
		}

		private List<Promocion> LeerDesdeDisco()
		{
			if (!File.Exists(_rutaArchivo)) return new List<Promocion>();
			var json = File.ReadAllText(_rutaArchivo);
			return JsonSerializer.Deserialize<List<Promocion>>(json, _opciones) ?? new List<Promocion>();
		}

		private void PersistirSinCandado(List<Promocion> promociones)
		{
			var json = JsonSerializer.Serialize(promociones, _opciones);
			var rutaTemporal = _rutaArchivo + ".tmp";
			File.WriteAllText(rutaTemporal, json);
			File.Move(rutaTemporal, _rutaArchivo, overwrite: true);
			_cache = promociones;
		}
	}
}