using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ProyectoJo.Domain.Entities;
using ProyectoJo.Application.Ports.In;

namespace ProyectoJo.Web.Controllers;
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;

	private readonly IProductoService _productoService;


	public HomeController(ILogger<HomeController> logger, IProductoService productService)
		{
			_logger = logger;
		_productoService = productService;
		}

		public IActionResult Index()
		{
			try
			{
				// 1. Intentamos obtener el menú
				var menu = _productoService.ObtenerMenu();

				// 2. Si el archivo no existe o está vacío, creamos una lista de emergencia
				if (menu == null || !menu.Any())
				{
					menu = new List<ProyectoJo.Domain.Entities.Item>();
				}

				// 3. Tomamos los últimos 3 
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