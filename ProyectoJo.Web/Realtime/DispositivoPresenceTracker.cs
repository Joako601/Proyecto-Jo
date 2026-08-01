using System.Collections.Concurrent;

namespace ProyectoJo.Web.Realtime
{
	public class DispositivoPresenceTracker
	{
		private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> _conexionesPorToken = new();

		public void Conectar(string token, string connectionId)
		{
			var conexiones = _conexionesPorToken.GetOrAdd(token, _ => new ConcurrentDictionary<string, byte>());
			conexiones[connectionId] = 0;
		}

		public void Desconectar(string token, string connectionId)
		{
			if (!_conexionesPorToken.TryGetValue(token, out var conexiones)) return;

			conexiones.TryRemove(connectionId, out _);
			if (conexiones.IsEmpty)
				_conexionesPorToken.TryRemove(token, out _);
		}

		public bool EstaConectado(string token) =>
			_conexionesPorToken.TryGetValue(token, out var conexiones) && !conexiones.IsEmpty;

		public IReadOnlyCollection<string> ObtenerConexiones(string token) =>
			_conexionesPorToken.TryGetValue(token, out var conexiones)
				? conexiones.Keys.ToList()
				: Array.Empty<string>();
	}
}
