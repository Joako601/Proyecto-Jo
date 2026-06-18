using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Infrastructure.Persistence
{
	public class JsonProductRepository : IProductoRepository
	{
		private readonly IWebHostEnvironment _env;

		public JsonProductRepository(IWebHostEnvironment env)
		{
			_env = env;
		}

		private string GetPath() =>
			Path.Combine(_env.ContentRootPath, "Persistencia", "menu.json");

		public IEnumerable<Item> ObtenerTodos() => LeerJson();

		public IEnumerable<Item> ObtenerPorCategoria(string categoria) =>
			LeerJson().Where(i => i.Categoria == categoria);

		public List<Item> ObtenerMenu() => LeerJson();

		public void GuardarMenu(List<Item> menu)
		{
			var json = JsonSerializer.Serialize(menu, new JsonSerializerOptions { WriteIndented = true });
			File.WriteAllText(GetPath(), json);
		}

		private List<Item> LeerJson()
		{
			var json = File.ReadAllText(GetPath());
			return JsonSerializer.Deserialize<List<Item>>(json) ?? new List<Item>();
		}
	}
}