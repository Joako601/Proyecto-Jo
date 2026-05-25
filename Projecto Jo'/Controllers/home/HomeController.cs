using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Proyecto_Jo_.Data; // Importante: agregamos esto para que reconozca el JsonProductService
using Proyecto_Jo_.Models;

namespace Proyecto_Jo_.Controllers // Namespace actualizado al nuevo proyecto
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		private readonly JsonProductService _productService; // Declaramos el servicio JSON

		// Inyectamos tanto el Logger como el JsonProductService
		public HomeController(ILogger<HomeController> logger, JsonProductService productService)
		{
			_logger = logger;
			_productService = productService;
		}

		public IActionResult Index()
		{
			try
			{
				// 1. Intentamos obtener el menú
				var menu = _productService.ObtenerMenu();

				// 2. Si el archivo no existe o está vacío, creamos una lista de emergencia
				if (menu == null || !menu.Any())
				{
					menu = new List<Proyecto_Jo_.Models.Item>();
				}

				// 3. Tomamos los últimos 3 (ahora es seguro porque sabemos que no es nulo)
				var itemsParaHome = menu.TakeLast(3).ToList();

				return View(itemsParaHome);
			}
			catch (Exception ex)
			{
				// 4. Si el JSON tiene un error de formato grave, lo mostramos en texto plano
				return Content("Error en el Backend al leer el JSON: " + ex.Message);
			}
		}

		public IActionResult Privacy()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}