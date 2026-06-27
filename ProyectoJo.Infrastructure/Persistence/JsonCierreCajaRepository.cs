using System.Text.Json;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Infrastructure.Persistence
{
	public class JsonCierreCajaRepository : ICierreCajaRepository
	{
		private readonly string _rutaArchivo;
		private static readonly object _lock = new();

		public JsonCierreCajaRepository(string rutaArchivo)
		{
			_rutaArchivo = rutaArchivo;
		}

		public List<CierreCaja> ObtenerTodos()
		{
			lock (_lock)
			{
				return LeerSinCandado();
			}
		}

		public CierreCaja? ObtenerPorId(int id) =>
			ObtenerTodos().FirstOrDefault(c => c.Id == id);

		public void Guardar(CierreCaja cierreCaja)
		{
			lock (_lock)
			{
				var lista = LeerSinCandado();
				lista.Add(cierreCaja);
				PersistirSinCandado(lista);
			}
		}

		public void Actualizar(CierreCaja cierreCaja)
		{
			lock (_lock)
			{
				var lista = LeerSinCandado();
				var index = lista.FindIndex(c => c.Id == cierreCaja.Id);
				if (index >= 0) lista[index] = cierreCaja;
				PersistirSinCandado(lista);
			}
		}

		private List<CierreCaja> LeerSinCandado()
		{
			if (!File.Exists(_rutaArchivo)) return new List<CierreCaja>();
			var json = File.ReadAllText(_rutaArchivo);
			return JsonSerializer.Deserialize<List<CierreCaja>>(json) ?? new List<CierreCaja>();
		}

		private void PersistirSinCandado(List<CierreCaja> lista)
		{
			var json = JsonSerializer.Serialize(lista, new JsonSerializerOptions { WriteIndented = true });
			File.WriteAllText(_rutaArchivo, json);
		}
	}
}