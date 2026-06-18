using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Infrastructure.Persistence
{
	public class JsonFinanzaRepository : IFinanzaRepository
	{
		private readonly IWebHostEnvironment _env;

		public JsonFinanzaRepository(IWebHostEnvironment env)
		{
			_env = env;
		}

		private string GetPath() =>
			Path.Combine(_env.ContentRootPath, "Persistencia", "finanzas.json");

		public List<Finanza> ObtenerTodos()
		{
			if (!File.Exists(GetPath())) return new List<Finanza>();
			var json = File.ReadAllText(GetPath());
			return JsonSerializer.Deserialize<List<Finanza>>(json) ?? new List<Finanza>();
		}

		public Finanza? ObtenerPorId(int id) =>
			ObtenerTodos().FirstOrDefault(f => f.Id == id);

		public void Guardar(Finanza finanza)
		{
			var lista = ObtenerTodos();
			lista.Add(finanza);
			Persistir(lista);
		}

		public void Actualizar(Finanza finanza)
		{
			var lista = ObtenerTodos();
			var index = lista.FindIndex(f => f.Id == finanza.Id);
			if (index >= 0) lista[index] = finanza;
			Persistir(lista);
		}

		public void Eliminar(int id)
		{
			var lista = ObtenerTodos();
			lista.RemoveAll(f => f.Id == id);
			Persistir(lista);
		}

		private void Persistir(List<Finanza> lista)
		{
			var json = JsonSerializer.Serialize(lista, new JsonSerializerOptions { WriteIndented = true });
			File.WriteAllText(GetPath(), json);
		}
	}
}