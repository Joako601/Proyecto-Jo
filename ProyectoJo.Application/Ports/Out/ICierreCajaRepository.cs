using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Application.Ports.Out
{
	public interface ICierreCajaRepository
	{
		List<CierreCaja> ObtenerTodos();
		CierreCaja? ObtenerPorId(int id);
		void Guardar(CierreCaja cierreCaja);
		void Actualizar(CierreCaja cierreCaja);
	}
}