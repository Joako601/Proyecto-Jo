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

		public void Editar(Promocion promocion)
		{
			lock (_lock)
			{
				var promociones = ObtenerCache();
				var index = promociones.FindIndex(p => p.Id == promocion.Id);
				if (index >= 0) promociones[index] = promocion;
				PersistirSinCandado(promociones);
			}
		}

		public void Eliminar(int id)
		{
			lock (_lock)
			{
				var promociones = ObtenerCache();
				promociones.RemoveAll(p => p.Id == id);
				PersistirSinCandado(promociones);
			}
		}

		public void ToggleActiva(int id)
		{
			lock (_lock)
			{
				var promociones = ObtenerCache();
				var promo = promociones.Find(p => p.Id == id);
				if (promo != null) promo.Activa = !promo.Activa;
				PersistirSinCandado(promociones);
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