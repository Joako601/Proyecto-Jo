using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Application.Ports.In
{
	public interface IInsumoService
	{
		List<Insumo> ObtenerTodos();
		Insumo? ObtenerPorId(int id);
		void Agregar(Insumo insumo, string usuario);
		bool Editar(Insumo insumo, string usuario);
		bool Eliminar(int id, string usuario);
		bool Reponer(int id, decimal cantidad, string usuario);

		Task<string?> VerificarYDescontarAsync(List<ItemPedido> items, Func<int, Receta?> obtenerRecetaPorItemId);

		int SincronizarDesdeMenu(IEnumerable<Item> menu, string usuario);

		int? ObtenerMaximoDisponible(Item item);
	}
}