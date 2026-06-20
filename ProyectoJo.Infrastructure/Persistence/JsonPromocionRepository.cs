using System.Text.Json;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Infrastructure.Persistence
{
	public class JsonPromocionRepository : IPromocionRepository
	{
		private readonly string _rutaArchivo;

		public JsonPromocionRepository(string rutaArchivo)
		{
			_rutaArchivo = rutaArchivo;
		}

		public IEnumerable<Promocion> ObtenerTodas() => LeerJson();

		public Promocion? ObtenerPorId(int id) =>
			LeerJson().FirstOrDefault(p => p.Id == id);

		public void Agregar(Promocion promocion)
		{
			var promociones = LeerJson();
			promociones.Add(promocion);
			Guardar(promociones);
		}

		public void Editar(Promocion promocion)
		{
			var promociones = LeerJson();
			var index = promociones.FindIndex(p => p.Id == promocion.Id);
			if (index >= 0) promociones[index] = promocion;
			Guardar(promociones);
		}

		public void Eliminar(int id)
		{
			var promociones = LeerJson();
			promociones.RemoveAll(p => p.Id == id);
			Guardar(promociones);
		}

		public void ToggleActiva(int id)
		{
			var promociones = LeerJson();
			var promo = promociones.FirstOrDefault(p => p.Id == id);
			if (promo != null) promo.Activa = !promo.Activa;
			Guardar(promociones);
		}

		private List<Promocion> LeerJson()
		{
			if (!File.Exists(_rutaArchivo)) return new List<Promocion>();
			var json = File.ReadAllText(_rutaArchivo);
			return JsonSerializer.Deserialize<List<Promocion>>(json) ?? new List<Promocion>();
		}

		private void Guardar(List<Promocion> promociones)
		{
			var json = JsonSerializer.Serialize(promociones, new JsonSerializerOptions { WriteIndented = true });
			File.WriteAllText(_rutaArchivo, json);
		}
	}
}