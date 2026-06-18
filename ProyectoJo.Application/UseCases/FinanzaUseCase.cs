using ProyectoJo.Application.DTOs;
using ProyectoJo.Application.Ports.In;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Application.UseCases
{
	public class FinanzaUseCase : IFinanzaService
	{
		private readonly IFinanzaRepository _repository;

		public FinanzaUseCase(IFinanzaRepository repository)
		{
			_repository = repository;
		}

		public void RegistrarMovimiento(Finanza finanza)
		{
			var todos = _repository.ObtenerTodos();
			finanza.Id = todos.Count > 0 ? todos.Max(f => f.Id) + 1 : 1;
			finanza.Fecha = finanza.Fecha == default ? DateTime.Now : finanza.Fecha;
			_repository.Guardar(finanza);
		}

		public List<Finanza> ObtenerTodos() => _repository.ObtenerTodos();

		public List<Finanza> ObtenerPorFecha(DateTime desde, DateTime hasta) =>
			_repository.ObtenerTodos()
				.Where(f => f.Fecha.Date >= desde.Date && f.Fecha.Date <= hasta.Date)
				.ToList();

		public List<Finanza> ObtenerPorCategoria(string categoria) =>
			_repository.ObtenerTodos()
				.Where(f => f.Categoria == categoria)
				.ToList();

		public ResumenFinanciero ObtenerResumenDelDia(DateTime fecha)
		{
			var movimientos = ObtenerPorFecha(fecha, fecha);
			return Calcular(movimientos, fecha, fecha);
		}

		public ResumenFinanciero ObtenerResumenPorPeriodo(DateTime desde, DateTime hasta)
		{
			var movimientos = ObtenerPorFecha(desde, hasta);
			return Calcular(movimientos, desde, hasta);
		}

		public Finanza? ObtenerPorId(int id) => _repository.ObtenerPorId(id);

		public void Editar(Finanza finanza) => _repository.Actualizar(finanza);

		public void Eliminar(int id) => _repository.Eliminar(id);

		private ResumenFinanciero Calcular(List<Finanza> movimientos, DateTime desde, DateTime hasta) =>
			new ResumenFinanciero
			{
				TotalIngresos = movimientos.Where(f => f.Tipo == TipoMovimiento.Ingreso).Sum(f => f.Monto),
				TotalEgresos = movimientos.Where(f => f.Tipo == TipoMovimiento.Egreso).Sum(f => f.Monto),
				Desde = desde,
				Hasta = hasta
			};
	}
}