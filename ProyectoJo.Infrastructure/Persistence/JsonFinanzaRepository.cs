using System.Text.Json;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Infrastructure.Persistence
{
	public class JsonFinanzaRepository : IFinanzaRepository
	{
		private readonly string _rutaArchivo;

		private static readonly List<Finanza> _lista = new();
		private static readonly Dictionary<int, int> _indice = new();
		private static readonly object _lock = new();
		private static bool _cargado;
		private static int _siguienteId = 1;

		private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

		public JsonFinanzaRepository(string rutaArchivo)
		{
			_rutaArchivo = rutaArchivo;
		}

		public List<Finanza> ObtenerTodos()
		{
			lock (_lock)
			{
				AsegurarCargado();
				return _lista;
			}
		}

		public Finanza? ObtenerPorId(int id)
		{
			lock (_lock)
			{
				AsegurarCargado();
				return _indice.TryGetValue(id, out var pos) ? _lista[pos] : null;
			}
		}

		public void Guardar(Finanza finanza)
		{
			lock (_lock)
			{
				AsegurarCargado();
				finanza.Id = _siguienteId++;
				_indice[finanza.Id] = _lista.Count;
				_lista.Add(finanza);
				Persistir();
			}
		}

		public void Actualizar(Finanza finanza)
		{
			lock (_lock)
			{
				AsegurarCargado();
				if (_indice.TryGetValue(finanza.Id, out var pos))
				{
					_lista[pos] = finanza;
					Persistir();
				}
			}
		}

		public void Eliminar(int id)
		{
			lock (_lock)
			{
				AsegurarCargado();
				if (!_indice.TryGetValue(id, out var pos)) return;

				var ultimo = _lista.Count - 1;
				var itemFinal = _lista[ultimo];
				_lista[pos] = itemFinal;
				_indice[itemFinal.Id] = pos;
				_lista.RemoveAt(ultimo);
				_indice.Remove(id);

				Persistir();
			}
		}

		private void AsegurarCargado()
		{
			if (_cargado) return;

			if (File.Exists(_rutaArchivo))
			{
				var json = File.ReadAllText(_rutaArchivo);
				var datos = JsonSerializer.Deserialize<List<Finanza>>(json) ?? new List<Finanza>();
				_lista.AddRange(datos);
				for (int i = 0; i < _lista.Count; i++)
					_indice[_lista[i].Id] = i;

				_siguienteId = _lista.Count > 0 ? _lista.Max(f => f.Id) + 1 : 1;
			}

			_cargado = true;
		}

		private void Persistir()
		{
			var json = JsonSerializer.Serialize(_lista, _jsonOptions);
			var rutaTemporal = _rutaArchivo + ".tmp";
			File.WriteAllText(rutaTemporal, json);
			File.Move(rutaTemporal, _rutaArchivo, overwrite: true);
		}
	}
}