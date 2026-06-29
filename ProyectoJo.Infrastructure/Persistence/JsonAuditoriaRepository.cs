using System.Text.Json;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Infrastructure.Persistence
{
	public class JsonAuditoriaRepository : IAuditoriaRepository
	{
		private readonly string _rutaArchivo;
		private static readonly object _lock = new();

		public JsonAuditoriaRepository(string rutaArchivo)
		{
			_rutaArchivo = rutaArchivo;
		}

		public List<RegistroAuditoria> ObtenerTodos()
		{
			lock (_lock)
			{
				return LeerSinCandado();
			}
		}

		public void Guardar(RegistroAuditoria registro)
		{
			lock (_lock)
			{
				var lista = LeerSinCandado();
				registro.Id = lista.Count > 0 ? lista.Max(r => r.Id) + 1 : 1;
				lista.Add(registro);
				PersistirSinCandado(lista);
			}
		}

		private List<RegistroAuditoria> LeerSinCandado()
		{
			if (!File.Exists(_rutaArchivo)) return new List<RegistroAuditoria>();
			var json = File.ReadAllText(_rutaArchivo);
			return JsonSerializer.Deserialize<List<RegistroAuditoria>>(json) ?? new List<RegistroAuditoria>();
		}

		private void PersistirSinCandado(List<RegistroAuditoria> lista)
		{
			var json = JsonSerializer.Serialize(lista, new JsonSerializerOptions { WriteIndented = true });
			var rutaTemporal = _rutaArchivo + ".tmp";
			File.WriteAllText(rutaTemporal, json);
			File.Move(rutaTemporal, _rutaArchivo, overwrite: true);
		}
	}
}
