using System.Text.Json;
using Proyecto_Jo_.Models;

namespace Proyecto_Jo_.Data
{
	public class JsonProductService
	{
		private readonly string _jsonFilePath;

		// El constructor genera la ruta hacia /data/menu.json
		public JsonProductService(IWebHostEnvironment webHostEnvironment)
		{
			string baseDir = AppDomain.CurrentDomain.BaseDirectory;

			
			_jsonFilePath = Path.Combine(webHostEnvironment.ContentRootPath, "Persistencia", "menu.json");

			
			var directory = Path.GetDirectoryName(_jsonFilePath);
			if (!Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
			}

			InicializarJsonSiNoExiste();
		}

		public List<Item> ObtenerMenu()
		{
			if (!File.Exists(_jsonFilePath)) return new List<Item>();

			var json = File.ReadAllText(_jsonFilePath);
			return JsonSerializer.Deserialize<List<Item>>(json) ?? new List<Item>();
		}

		public void GuardarMenu(List<Item> menu)
		{
			var opciones = new JsonSerializerOptions { WriteIndented = true };
			var json = JsonSerializer.Serialize(menu, opciones);
			File.WriteAllText(_jsonFilePath, json);
		}

		private void InicializarJsonSiNoExiste()
		{
			var directory = Path.GetDirectoryName(_jsonFilePath);
			if (!Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
			}

			if (!File.Exists(_jsonFilePath))
			{
				
				var menuInicial = new List<Item>
				{
					new Item { Id = 1, Platillo = "Panucho Tradicional", Categoria = "Panuchos", Base = "Frijol y Pavo", Precio = 25, Descripcion = "Tortilla frita rellena de frijol, con pavo, repollo, tomate, pepino y cebolla." },
					new Item { Id = 2, Platillo = "Panucho de Huevo", Categoria = "Panuchos", Base = "Frijol y Huevo", Precio = 20, Descripcion = "Tortilla frita rellena de frijol, huevo cocido, repollo, tomate y cebolla." },
					new Item { Id = 3, Platillo = "Salbute de Pavo", Categoria = "Salbutes", Base = "Masa Frita", Precio = 25, Descripcion = "Lechuga, repollo, pavo, tomate, pepino, aguacate y cebolla." },
					new Item { Id = 4, Platillo = "Salbute de Relleno Negro", Categoria = "Salbutes", Base = "Masa Frita", Precio = 30, Descripcion = "Tradicional relleno negro yucateco con huevo y carne." },
					new Item { Id = 5, Platillo = "Torta de Carne Asada con Queso", Categoria = "Tortas", Base = "Pan Francés", Precio = 60, Descripcion = "Lechuga, carne asada, cebolla, aguacate y queso fundido." },
					new Item { Id = 6, Platillo = "Torta Especial Tía Caro", Categoria = "Tortas", Base = "Pan Francés", Precio = 70, Descripcion = "Carne asada, pavo, lechuga, cebolla, aguacate y queso." },
					new Item { Id = 7, Platillo = "Caldo Comensal", Categoria = "Caldos", Base = "Caldo de Pavo", Precio = 80, Descripcion = "Pavo, repollo, pepino, cebolla, cilantro y tostadas." },
					new Item { Id = 8, Platillo = "Agua de Chaya con Limón", Categoria = "Bebidas", Base = "Natural", Precio = 30, Descripcion = "Refrescante bebida tradicional yucateca." },
					new Item { Id = 9, Platillo = "Refresco Embotellado", Categoria = "Bebidas", Base = "Gasificada", Precio = 25, Descripcion = "Coca-Cola, Cristal o Sidral (600ml)." }
				};
				GuardarMenu(menuInicial);
			}
		}
	}
}