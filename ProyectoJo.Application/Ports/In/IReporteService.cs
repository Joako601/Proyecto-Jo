using ProyectoJo.Application.DTOs;

namespace ProyectoJo.Application.Ports.In
{
	public interface IReporteService
	{
		Task<ResumenMapaCalor> ObtenerMapaCalorAsync(
			DateTime? desde = null,
			DateTime? hasta = null,
			bool semanaHistoricoCompleto = true,
			int semanaOffset = 0,
			int? anioMeses = null,
			int? mesDetalle = null);
	}
}