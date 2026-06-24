using Microsoft.AspNetCore.Mvc;
using ProyectoJo.Application.Ports.In;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Api.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class ProductosController : ControllerBase
	{
		private readonly IProductoService _productoService;

		public ProductosController(IProductoService productoService)
		{
			_productoService = productoService;
		}

		[HttpGet("menu")]
		[Tags("Menú")]
		[ProducesResponseType(typeof(List<Item>), StatusCodes.Status200OK)]
		public IActionResult GetMenu()
		{
			var menu = _productoService.ObtenerMenu();
			return Ok(menu);
		}
	}
}