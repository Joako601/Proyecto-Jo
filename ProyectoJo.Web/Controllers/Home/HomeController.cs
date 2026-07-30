using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ProyectoJo.Domain.Entities;
using ProyectoJo.Application.Ports.In;
using ProyectoJo.Web.Models;

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
		var menu = _productoService.ObtenerMenu() ?? new List<Item>();
		var itemsParaHome = menu.TakeLast(3).ToList();

		return View(itemsParaHome);
	}

	public IActionResult Privacy()
	{
		return View();
	}

	[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
	public IActionResult Error()
	{
		var feature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

		if (feature?.Error is not null)
		{
			_logger.LogError(feature.Error,
				"Error no controlado en la ruta {Ruta}", feature.Path);
		}

		return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
	}
}