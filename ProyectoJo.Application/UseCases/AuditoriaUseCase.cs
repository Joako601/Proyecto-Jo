using ProyectoJo.Application.Ports.In;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Application.UseCases
{
	public class AuditoriaUseCase : IAuditoriaService
	{
		private readonly IAuditoriaRepository _auditoriaRepository;

		public AuditoriaUseCase(IAuditoriaRepository auditoriaRepository)
		{
			_auditoriaRepository = auditoriaRepository;
		}

		public void RegistrarAccion(string usuario, string modulo, TipoAccionAuditoria accion, string entidad,
			string? detalleAntes = null, string? detalleDespues = null)
		{
			var registro = new RegistroAuditoria
			{
				FechaHora = DateTime.Now,
				Usuario = usuario,
				Modulo = modulo,
				Accion = accion,
				Entidad = entidad,
				DetalleAntes = detalleAntes,
				DetalleDespues = detalleDespues
			};

			_auditoriaRepository.Guardar(registro);
		}

		public List<RegistroAuditoria> ObtenerHistorial(string? modulo = null, DateTime? desde = null, DateTime? hasta = null)
		{
			var registros = _auditoriaRepository.ObtenerTodos().AsEnumerable();

			if (!string.IsNullOrWhiteSpace(modulo))
				registros = registros.Where(r => string.Equals(r.Modulo, modulo, StringComparison.OrdinalIgnoreCase));

			if (desde.HasValue)
				registros = registros.Where(r => r.FechaHora.Date >= desde.Value.Date);

			if (hasta.HasValue)
				registros = registros.Where(r => r.FechaHora.Date <= hasta.Value.Date);

			return registros.OrderByDescending(r => r.FechaHora).ToList();
		}
	}
}