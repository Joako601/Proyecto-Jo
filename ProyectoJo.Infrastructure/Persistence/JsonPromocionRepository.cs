using System.Text.Json;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Infrastructure.Persistence
{
	public class JsonPromocionRepository : IPromocionRepository
	{
		private readonly string _rutaArchivo;
		private static readonly object _lock = new();

		public JsonPromocionRepository(string rutaArchivo)
		{
			_rutaArchivo = rutaArchivo;
		}

		public IEnumerable<Promocion> ObtenerTodas()
		{
			lock (_lock)
			{
				return LeerSinCandado();
			}
		}

		public Promocion? ObtenerPorId(int id)
		{
			lock (_lock)
			{
				return LeerSinCandado().FirstOrDefault(p => p.Id == id);
			}
		}

		public void Agregar(Promocion promocion)
		{
			lock (_lock)
			{
				var promociones = LeerSinCandado();
				promocion.Id = promociones.Count > 0 ? promociones.Max(p => p.Id) + 1 : 1;
				promociones.Add(promocion);
				PersistirSinCandado(promociones);
			}
		}

		public void Editar(Promocion promocion)
		{
			lock (_lock)
			{
				var promociones = LeerSinCandado();
				var index = promociones.FindIndex(p => p.Id == promocion.Id);
				if (index >= 0) promociones[index] = promocion;
				PersistirSinCandado(promociones);
			}
		}

		public void Eliminar(int id)
		{
			lock (_lock)
			{
				var promociones = LeerSinCandado();
				promociones.RemoveAll(p => p.Id == id);
				PersistirSinCandado(promociones);
			}
		}

		public void ToggleActiva(int id)
		{
			lock (_lock)
			{
				var promociones = LeerSinCandado();
				var promo = promociones.FirstOrDefault(p => p.Id == id);
				if (promo != null) promo.Activa = !promo.Activa;
				PersistirSinCandado(promociones);
			}
		}

		private List<Promocion> LeerSinCandado()
		{
			if (!File.Exists(_rutaArchivo)) return new List<Promocion>();
			var json = File.ReadAllText(_rutaArchivo);
			return JsonSerializer.Deserialize<List<Promocion>>(json) ?? new List<Promocion>();
		}

		private void PersistirSinCandado(List<Promocion> promociones)
		{
			var json = JsonSerializer.Serialize(promociones, new JsonSerializerOptions { WriteIndented = true });
			File.WriteAllText(_rutaArchivo, json);
		}
	}
}