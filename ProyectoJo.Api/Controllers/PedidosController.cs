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

		
		[HttpGet]
		[ProducesResponseType(typeof(List<Pedido>), StatusCodes.Status200OK)]
		public async Task<IActionResult> GetPendientes()
		{
			var pedidos = await _pedidoService.ObtenerPendientesAsync();
			return Ok(pedidos);
		}

		
		[HttpGet("{id}")]
		[ProducesResponseType(typeof(Pedido), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> GetById(int id)
		{
			var pedido = await _pedidoService.ObtenerPorIdAsync(id);
			if (pedido is null) return NotFound();
			return Ok(pedido);
		}

		
		[HttpPost]
		[ProducesResponseType(typeof(Pedido), StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> Create([FromBody] Pedido pedido)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);
			var creado = await _pedidoService.CrearAsync(pedido);
			return CreatedAtAction(nameof(GetById), new { id = creado.Id }, creado);
		}

		/// <summary>
		/// Updates the order status. Kitchen uses this to mark an order as Prepared.
		/// </summary>
		[HttpPatch("{id}/estado")]
		[ProducesResponseType(typeof(Pedido), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> CambiarEstado(int id, [FromBody] EstadoPedido nuevoEstado)
		{
			var actualizado = await _pedidoService.CambiarEstadoAsync(id, nuevoEstado);
			if (actualizado is null) return NotFound();
			return Ok(actualizado);
		}

		
		[HttpPatch("{id}/pagar")]
		[ProducesResponseType(typeof(Pedido), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> Pagar(int id)
		{
			var actualizado = await _pedidoService.CambiarEstadoAsync(id, EstadoPedido.Pagado);
			if (actualizado is null) return NotFound();
			return Ok(actualizado);
		}
	}
}