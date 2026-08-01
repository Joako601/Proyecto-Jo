using ProyectoJo.Domain.Entities;

public interface IPedidoRepository
{
	Task<List<Pedido>> ObtenerTodosAsync();
	Task<List<Pedido>> ObtenerActivosAsync();
	Task<List<Pedido>> ObtenerDelDiaAsync(DateTime desde);
	Task<Pedido?> ObtenerPorIdAsync(int id);
	Task<Pedido> GuardarAsync(Pedido pedido);
	Task<Pedido?> ActualizarAsync(Pedido pedido);
	Task<(Pedido? Anterior, Pedido? Actualizado, string? MotivoRechazo)> CambiarEstadoAtomicoAsync(
		int id,
		EstadoPedido nuevoEstado,
		Func<Pedido, Task<string?>>? validarAntesDeAplicar = null);
}