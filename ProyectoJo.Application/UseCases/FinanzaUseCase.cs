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
			_repository.ObtenerPorFecha(desde, hasta);

		public (List<Finanza> Items, int Total) ObtenerPaginado(int mes, int anio, int pagina, int porPagina) =>
			_repository.ObtenerPaginado(mes, anio, pagina, porPagina);

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

			var porMesTipo = todos
				.GroupBy(f => (f.Fecha.Year, f.Fecha.Month, f.Tipo))
				.ToDictionary(g => g.Key, g => (Total: g.Sum(f => f.Monto), Cantidad: g.Count()));

			decimal Monto(int anio, int mes, TipoMovimiento tipo) =>
				porMesTipo.TryGetValue((anio, mes, tipo), out var v) ? v.Total : 0;

			int Cantidad(int anio, int mes, TipoMovimiento tipo) =>
				porMesTipo.TryGetValue((anio, mes, tipo), out var v) ? v.Cantidad : 0;

			var ventasAnio = porMesTipo
				.Where(kv => kv.Key.Year == hoy.Year && kv.Key.Tipo == TipoMovimiento.Ingreso)
				.Sum(kv => kv.Value.Total);
			var ventasMes = Monto(hoy.Year, hoy.Month, TipoMovimiento.Ingreso);
			var ventasDia = todos
				.Where(f => f.Fecha.Date == hoy.Date && f.Tipo == TipoMovimiento.Ingreso)
				.Sum(f => f.Monto);

			var ventasMesPasado = Monto(mesPasado.Year, mesPasado.Month, TipoMovimiento.Ingreso);
			var cantidadMesPasado = Cantidad(mesPasado.Year, mesPasado.Month, TipoMovimiento.Ingreso);
			var ticketPromedioMesPasado = cantidadMesPasado > 0
				? ventasMesPasado / cantidadMesPasado
				: 0;

			var tendenciaAnio = Enumerable.Range(1, 12)
				.Select(mes => new DateTime(hoy.Year, mes, 1))
				.Select(fecha => new TendenciaMensual
				{
					Mes = fecha.Month,
					Anio = fecha.Year,
					Etiqueta = fecha.ToString("MMM", cultura),
					Ingresos = Monto(fecha.Year, fecha.Month, TipoMovimiento.Ingreso),
					Egresos = Monto(fecha.Year, fecha.Month, TipoMovimiento.Egreso)
				})
				.ToList();

			var ultimosSeisMeses = Enumerable.Range(0, 6)
				.Select(i => hoy.AddMonths(-i))
				.Select(fecha => new TendenciaMensual
				{
					Mes = fecha.Month,
					Anio = fecha.Year,
					Etiqueta = fecha.ToString("MMM yyyy", cultura),
					Ingresos = Monto(fecha.Year, fecha.Month, TipoMovimiento.Ingreso),
					Egresos = Monto(fecha.Year, fecha.Month, TipoMovimiento.Egreso)
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
				TotalIngresosHistorico = porMesTipo.Where(kv => kv.Key.Tipo == TipoMovimiento.Ingreso).Sum(kv => kv.Value.Total),
				TotalEgresosHistorico = porMesTipo.Where(kv => kv.Key.Tipo == TipoMovimiento.Egreso).Sum(kv => kv.Value.Total),
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