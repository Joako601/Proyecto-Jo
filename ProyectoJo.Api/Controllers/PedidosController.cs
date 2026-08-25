using Microsoft.AspNetCore.Mvc;
using ProyectoJo.Application.Ports.In;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Api.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class PedidosController : ControllerBase
	{
		private readonly IPedidoService _pedidoService;

		public PedidosController(IPedidoService pedidoService)
		{
			_pedidoService = pedidoService;
		}


		[HttpGet("recepcion")]
		[Tags("Recepción")]
		[ProducesResponseType(typeof(List<Pedido>), StatusCodes.Status200OK)]
		public async Task<IActionResult> GetRecepcion()
		{
			var pedidos = await _pedidoService.ObtenerParaRecepcionAsync();
			return Ok(pedidos);
		}


		[HttpGet("{id}")]
		[Tags("Recepción")]
		[ProducesResponseType(typeof(Pedido), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> GetById(int id)
		{
			var pedido = await _pedidoService.ObtenerPorIdAsync(id);
			if (pedido is null) return NotFound();
			return Ok(pedido);
		}


		[HttpPost]
		[Tags("Recepción")]
		[ProducesResponseType(typeof(Pedido), StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> Create([FromBody] Pedido pedido)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);
			pedido.DescartarId();
			var resultado = await _pedidoService.CrearAsync(pedido, "API", "Recepcion");
			return CreatedAtAction(nameof(GetById), new { id = resultado.Pedido.Id }, resultado.Pedido);
		}


		[HttpPatch("{id}/pagar")]
		[Tags("Recepción")]
		[ProducesResponseType(typeof(Pedido), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status409Conflict)]
		public async Task<IActionResult> Pagar(int id)
		{
			var resultado = await _pedidoService.CambiarEstadoAsync(id, EstadoPedido.Pagado, "API", "Recepcion");
			if (resultado.NoEncontrado) return NotFound();
			if (!resultado.Exitoso) return Conflict(resultado.MotivoRechazo);
			return Ok(resultado.Pedido);
		}

		

		
		[HttpGet("cocina")]
		[Tags("Cocina")]
		[ProducesResponseType(typeof(List<Pedido>), StatusCodes.Status200OK)]
		public async Task<IActionResult> GetCocina()
		{
			var pedidos = await _pedidoService.ObtenerParaCocinaAsync();
			return Ok(pedidos);
		}

		
		[HttpPatch("{id}/estado")]
		[Tags("Cocina")]
		[ProducesResponseType(typeof(Pedido), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status409Conflict)]
		public async Task<IActionResult> CambiarEstado(int id, [FromBody] EstadoPedido nuevoEstado)
		{
			var resultado = await _pedidoService.CambiarEstadoAsync(id, nuevoEstado, "API", "Cocina");
			if (resultado.NoEncontrado) return NotFound();
			if (!resultado.Exitoso) return Conflict(resultado.MotivoRechazo);
			return Ok(resultado.Pedido);
		}
	}
}