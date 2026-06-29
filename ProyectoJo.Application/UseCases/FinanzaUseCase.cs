using ProyectoJo.Application.DTOs;
using ProyectoJo.Application.Ports.In;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Application.UseCases
{
	public class FinanzaUseCase : IFinanzaService
	{
		private readonly IFinanzaRepository _repository;
		private readonly IAuditoriaService _auditoriaService;

		public FinanzaUseCase(IFinanzaRepository repository, IAuditoriaService auditoriaService)
		{
			_repository = repository;
			_auditoriaService = auditoriaService;
		}

		public void RegistrarMovimiento(Finanza finanza, string usuario)
		{
			finanza.Fecha = finanza.Fecha == default ? DateTime.Now : finanza.Fecha;
			_repository.Guardar(finanza);

			_auditoriaService.RegistrarAccion(
				usuario: usuario,
				modulo: "Finanzas",
				accion: TipoAccionAuditoria.Creacion,
				entidad: $"Finanza #{finanza.Id}",
				detalleDespues: $"{finanza.Tipo} - {finanza.Categoria} - ${finanza.Monto}"
			);
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

		public ResumenDashboard ObtenerDashboard()
		{
			var cultura = System.Globalization.CultureInfo.GetCultureInfo("es-MX");
			var hoy = DateTime.Today;
			var mesPasado = hoy.AddMonths(-1);
			var todos = _repository.ObtenerTodos();

			decimal SumaIngresos(IEnumerable<Finanza> lista) =>
				lista.Where(f => f.Tipo == TipoMovimiento.Ingreso).Sum(f => f.Monto);

			var ventasAnio = SumaIngresos(todos.Where(f => f.Fecha.Year == hoy.Year));
			var ventasMes = SumaIngresos(todos.Where(f => f.Fecha.Year == hoy.Year && f.Fecha.Month == hoy.Month));
			var ventasDia = SumaIngresos(todos.Where(f => f.Fecha.Date == hoy.Date));

			var movimientosMesPasado = todos
				.Where(f => f.Fecha.Year == mesPasado.Year && f.Fecha.Month == mesPasado.Month && f.Tipo == TipoMovimiento.Ingreso)
				.ToList();
			var ventasMesPasado = movimientosMesPasado.Sum(f => f.Monto);
			var ticketPromedioMesPasado = movimientosMesPasado.Count > 0
				? ventasMesPasado / movimientosMesPasado.Count
				: 0;

			var tendenciaAnio = Enumerable.Range(1, 12)
				.Select(mes => new DateTime(hoy.Year, mes, 1))
				.Select(fecha => new TendenciaMensual
				{
					Mes = fecha.Month,
					Anio = fecha.Year,
					Etiqueta = fecha.ToString("MMM", cultura),
					Ingresos = todos.Where(f => f.Fecha.Month == fecha.Month && f.Fecha.Year == fecha.Year && f.Tipo == TipoMovimiento.Ingreso).Sum(f => f.Monto),
					Egresos = todos.Where(f => f.Fecha.Month == fecha.Month && f.Fecha.Year == fecha.Year && f.Tipo == TipoMovimiento.Egreso).Sum(f => f.Monto)
				})
				.ToList();

			var ultimosSeisMeses = Enumerable.Range(0, 6)
				.Select(i => hoy.AddMonths(-i))
				.Select(fecha => new TendenciaMensual
				{
					Mes = fecha.Month,
					Anio = fecha.Year,
					Etiqueta = fecha.ToString("MMM yyyy", cultura),
					Ingresos = todos.Where(f => f.Fecha.Month == fecha.Month && f.Fecha.Year == fecha.Year && f.Tipo == TipoMovimiento.Ingreso).Sum(f => f.Monto),
					Egresos = todos.Where(f => f.Fecha.Month == fecha.Month && f.Fecha.Year == fecha.Year && f.Tipo == TipoMovimiento.Egreso).Sum(f => f.Monto)
				})
				.OrderBy(t => t.Anio).ThenBy(t => t.Mes)
				.ToList();

			List<CategoriaResumen> TopCategoriasPorTipo(TipoMovimiento tipo) =>
				todos.Where(f => f.Tipo == tipo)
					.GroupBy(f => f.Categoria)
					.Select(g => new CategoriaResumen
					{
						Categoria = g.Key,
						Total = g.Sum(f => f.Monto),
						Cantidad = g.Count()
					})
					.OrderByDescending(c => c.Total)
					.Take(5)
					.ToList();

			return new ResumenDashboard
			{
				TotalIngresosHistorico = SumaIngresos(todos),
				TotalEgresosHistorico = todos.Where(f => f.Tipo == TipoMovimiento.Egreso).Sum(f => f.Monto),
				TotalMovimientos = todos.Count,
				VentasAnio = ventasAnio,
				VentasMes = ventasMes,
				VentasDia = ventasDia,
				VentasMesPasado = ventasMesPasado,
				TicketPromedioMesPasado = ticketPromedioMesPasado,
				TendenciaAnio = tendenciaAnio,
				UltimosSeisMeses = ultimosSeisMeses,
				TopCategorias = TopCategoriasPorTipo(TipoMovimiento.Egreso),
				TopCategoriasIngresos = TopCategoriasPorTipo(TipoMovimiento.Ingreso)
			};
		}

		public Finanza? ObtenerPorId(int id) => _repository.ObtenerPorId(id);

		public bool Editar(Finanza finanza, string usuario)
		{
			var anterior = _repository.ObtenerPorId(finanza.Id);
			if (anterior is null) return false;

			_repository.Actualizar(finanza);

			_auditoriaService.RegistrarAccion(
				usuario: usuario,
				modulo: "Finanzas",
				accion: TipoAccionAuditoria.Edicion,
				entidad: $"Finanza #{finanza.Id}",
				detalleAntes: $"{anterior.Tipo} - {anterior.Categoria} - ${anterior.Monto}",
				detalleDespues: $"{finanza.Tipo} - {finanza.Categoria} - ${finanza.Monto}"
			);

			return true;
		}

		public bool Eliminar(int id, string usuario)
		{
			var finanza = _repository.ObtenerPorId(id);
			if (finanza is null) return false;

			_repository.Eliminar(id);

			_auditoriaService.RegistrarAccion(
				usuario: usuario,
				modulo: "Finanzas",
				accion: TipoAccionAuditoria.Eliminacion,
				entidad: $"Finanza #{id}",
				detalleAntes: $"{finanza.Tipo} - {finanza.Categoria} - ${finanza.Monto}"
			);

			return true;
		}

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