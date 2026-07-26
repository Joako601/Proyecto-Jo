using System.Text.Json;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Infrastructure.Persistence
{
	public class JsonOpinionRepository : IOpinionRepository
	{
		private readonly string _rutaArchivo;
		private static readonly object _lock = new();
		private static readonly JsonSerializerOptions _opciones = new() { WriteIndented = false };

		private List<OpinionCliente>? _cache;

		public JsonOpinionRepository(string rutaArchivo)
		{
			_rutaArchivo = rutaArchivo;
		}

		public List<OpinionCliente> ObtenerTodas()
		{
			lock (_lock) { return ObtenerCache().ToList(); }
		}

		public OpinionCliente? ObtenerPorId(int id)
		{
			lock (_lock) { return ObtenerCache().Find(o => o.Id == id); }
		}

		public void Agregar(OpinionCliente opinion)
		{
			lock (_lock)
			{
				var opiniones = ObtenerCache();
				opinion.Id = opiniones.Count > 0 ? opiniones.Max(o => o.Id) + 1 : 1;
				opiniones.Add(opinion);
				PersistirSinCandado(opiniones);
			}
		}

		public bool Editar(OpinionCliente opinion)
		{
			lock (_lock)
			{
				var opiniones = ObtenerCache();
				var index = opiniones.FindIndex(o => o.Id == opinion.Id);
				if (index < 0) return false;

				opiniones[index] = opinion;
				PersistirSinCandado(opiniones);
				return true;
			}
		}

		public bool Eliminar(int id)
		{
			lock (_lock)
			{
				var opiniones = ObtenerCache();
				var eliminadas = opiniones.RemoveAll(o => o.Id == id);
				if (eliminadas == 0) return false;

				PersistirSinCandado(opiniones);
				return true;
			}
		}

		private List<OpinionCliente> ObtenerCache()
		{
			_cache ??= LeerDesdeDisco();
			return _cache;
		}

		private List<OpinionCliente> LeerDesdeDisco()
		{
			if (!File.Exists(_rutaArchivo)) return new List<OpinionCliente>();
			var json = File.ReadAllText(_rutaArchivo);
			return JsonSerializer.Deserialize<List<OpinionCliente>>(json, _opciones) ?? new List<OpinionCliente>();
		}

		private void PersistirSinCandado(List<OpinionCliente> opiniones)
		{
			var json = JsonSerializer.Serialize(opiniones, _opciones);
			var rutaTemporal = _rutaArchivo + ".tmp";
			File.WriteAllText(rutaTemporal, json);
			File.Move(rutaTemporal, _rutaArchivo, overwrite: true);
			_cache = opiniones;
		}
	}
}