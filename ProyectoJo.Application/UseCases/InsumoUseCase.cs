using ProyectoJo.Application.Ports.In;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Application.UseCases
{
	public class InsumoUseCase : IInsumoService
	{
		private readonly IInsumoRepository _repository;
		private readonly IAuditoriaService _auditoriaService;

		public InsumoUseCase(IInsumoRepository repository, IAuditoriaService auditoriaService)
		{
			_repository = repository;
			_auditoriaService = auditoriaService;
		}

		public List<Insumo> ObtenerTodos() => _repository.ObtenerTodos();

		public Insumo? ObtenerPorId(int id) => _repository.ObtenerPorId(id);

		public void Agregar(Insumo insumo, string usuario)
		{
			_repository.Agregar(insumo);
			_auditoriaService.RegistrarAccion(
				usuario: usuario,
				modulo: "Insumos",
				accion: TipoAccionAuditoria.Creacion,
				entidad: $"Insumo #{insumo.Id} - {insumo.Nombre}",
				detalleDespues: $"Stock inicial: {insumo.StockActual} {insumo.Unidad}"
			);
		}

		public bool Editar(Insumo insumo, string usuario)
		{
			var anterior = _repository.ObtenerPorId(insumo.Id);
			if (anterior is null) return false;

			var actualizado = _repository.Editar(insumo);
			if (!actualizado) return false;

			_auditoriaService.RegistrarAccion(
				usuario: usuario,
				modulo: "Insumos",
				accion: TipoAccionAuditoria.Edicion,
				entidad: $"Insumo #{insumo.Id} - {insumo.Nombre}",
				detalleAntes: $"Stock: {anterior.StockActual} {anterior.Unidad}, mínimo: {anterior.StockMinimo}",
				detalleDespues: $"Stock: {insumo.StockActual} {insumo.Unidad}, mínimo: {insumo.StockMinimo}"
			);

			return true;
		}

		public bool Eliminar(int id, string usuario)
		{
			var insumo = _repository.ObtenerPorId(id);
			if (insumo is null) return false;

			var eliminado = _repository.Eliminar(id);
			if (!eliminado) return false;

			_auditoriaService.RegistrarAccion(
				usuario: usuario,
				modulo: "Insumos",
				accion: TipoAccionAuditoria.Eliminacion,
				entidad: $"Insumo #{id} - {insumo.Nombre}",
				detalleAntes: $"Stock: {insumo.StockActual} {insumo.Unidad}"
			);

			return true;
		}

		public bool Reponer(int id, decimal cantidad, string usuario)
		{
			if (cantidad <= 0) return false;

			var anterior = _repository.ObtenerPorId(id);
			if (anterior is null) return false;

			var actualizado = _repository.ReponerAtomicoAsync(id, cantidad).GetAwaiter().GetResult();
			if (actualizado is null) return false;

			_auditoriaService.RegistrarAccion(
				usuario: usuario,
				modulo: "Insumos",
				accion: TipoAccionAuditoria.Edicion,
				entidad: $"Insumo #{id} - {actualizado.Nombre}",
				detalleAntes: $"Stock: {anterior.StockActual} {anterior.Unidad}",
				detalleDespues: $"Stock: {actualizado.StockActual} {actualizado.Unidad} (+{cantidad})"
			);

			return true;
		}

		public async Task<string?> VerificarYDescontarAsync(List<ItemPedido> items, Func<int, Receta?> obtenerRecetaPorItemId)
		{
			var consumoPorInsumoId = new Dictionary<int, decimal>();

			foreach (var linea in items)
			{
				var receta = obtenerRecetaPorItemId(linea.ItemId);
				if (receta is null) continue; // sin receta cargada = no controla stock para ese item

				foreach (var ingrediente in receta.Ingredientes)
				{
					var necesario = ingrediente.Cantidad * linea.Cantidad;
					consumoPorInsumoId[ingrediente.InsumoId] =
						consumoPorInsumoId.GetValueOrDefault(ingrediente.InsumoId) + necesario;
				}
			}

			if (consumoPorInsumoId.Count == 0) return null; // ningún item de este pedido tiene receta con insumos

			var (exitoso, faltantes) = await _repository.DescontarAtomicoAsync(consumoPorInsumoId);
			if (exitoso) return null;

			var detalle = string.Join("; ", faltantes.Select(f =>
				$"{f.Nombre}: hay {f.Disponible}, se necesitan {f.Necesario}"));

			return $"Stock insuficiente para preparar este pedido. {detalle}";
		}
	}
}