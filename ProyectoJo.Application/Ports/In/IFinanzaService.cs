using ProyectoJo.Application.DTOs;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Application.Ports.In
{
	public interface IFinanzaService
	{
		void RegistrarMovimiento(Finanza finanza, string usuario);
		List<Finanza> ObtenerTodos();
		List<Finanza> ObtenerPorFecha(DateTime desde, DateTime hasta);
		List<Finanza> ObtenerPorCategoria(string categoria);
		ResumenFinanciero ObtenerResumenDelDia(DateTime fecha);
		ResumenFinanciero ObtenerResumenPorPeriodo(DateTime desde, DateTime hasta);
		ResumenDashboard ObtenerDashboard();
		Finanza? ObtenerPorId(int id);
		void Editar(Finanza finanza, string usuario);
		void Eliminar(int id, string usuario);
	}
}