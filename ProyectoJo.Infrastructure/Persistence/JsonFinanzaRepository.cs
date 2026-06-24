using System.Text.Json;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Infrastructure.Persistence
{
	public class JsonFinanzaRepository : IFinanzaRepository
	{
		private readonly string _rutaArchivo;
		private static readonly object _lock = new();

		public JsonFinanzaRepository(string rutaArchivo)
		{
			_rutaArchivo = rutaArchivo;
		}

		public List<Finanza> ObtenerTodos()
		{
			lock (_lock)
			{
				return LeerSinCandado();
			}
		}

		public Finanza? ObtenerPorId(int id) =>
			ObtenerTodos().FirstOrDefault(f => f.Id == id);

		public void Guardar(Finanza finanza)
		{
			lock (_lock)
			{
				var lista = LeerSinCandado();
				lista.Add(finanza);
				PersistirSinCandado(lista);
			}
		}

		public void Actualizar(Finanza finanza)
		{
			lock (_lock)
			{
				var lista = LeerSinCandado();
				var index = lista.FindIndex(f => f.Id == finanza.Id);
				if (index >= 0) lista[index] = finanza;
				PersistirSinCandado(lista);
			}
		}

		public void Eliminar(int id)
		{
			lock (_lock)
			{
				var lista = LeerSinCandado();
				lista.RemoveAll(f => f.Id == id);
				PersistirSinCandado(lista);
			}
		}

		private List<Finanza> LeerSinCandado()
		{
			if (!File.Exists(_rutaArchivo)) return new List<Finanza>();
			var json = File.ReadAllText(_rutaArchivo);
			return JsonSerializer.Deserialize<List<Finanza>>(json) ?? new List<Finanza>();
		}

		private void Persistir(List<Finanza> lista)
		{
			lock (_lock)
			{
				PersistirSinCandado(lista);
			}
		}

		private void PersistirSinCandado(List<Finanza> lista)
		{
			var json = JsonSerializer.Serialize(lista, new JsonSerializerOptions { WriteIndented = true });
			File.WriteAllText(_rutaArchivo, json);
		}
	}
}