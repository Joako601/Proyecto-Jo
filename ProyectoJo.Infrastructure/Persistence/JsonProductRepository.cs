using System.Text.Json;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Infrastructure.Persistence
{
	public class JsonProductRepository : IProductoRepository
	{
		private readonly string _rutaArchivo;

		public JsonProductRepository(string rutaArchivo)
		{
			_rutaArchivo = rutaArchivo;
		}

		public IEnumerable<Item> ObtenerTodos() => LeerJson();

		public IEnumerable<Item> ObtenerPorCategoria(string categoria) =>
			LeerJson().Where(i => i.Categoria == categoria);

		public List<Item> ObtenerMenu() => LeerJson();

		public void GuardarMenu(List<Item> menu)
		{
			var json = JsonSerializer.Serialize(menu, new JsonSerializerOptions { WriteIndented = true });
			File.WriteAllText(_rutaArchivo, json);
		}

		public void AgregarItem(Item item)
		{
			var menu = LeerJson();
			menu.Add(item);
			var json = JsonSerializer.Serialize(menu, new JsonSerializerOptions { WriteIndented = true });
			File.WriteAllText(_rutaArchivo, json);
		}

		private List<Item> LeerJson()
		{
			var json = File.ReadAllText(_rutaArchivo);
			return JsonSerializer.Deserialize<List<Item>>(json) ?? new List<Item>();
		}
	}
}