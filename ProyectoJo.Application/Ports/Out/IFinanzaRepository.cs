using ProyectoJo.Domain.Entities;


namespace ProyectoJo.Application.Ports.Out
{
	public interface IFinanzaRepository
	{
		List<Finanza> ObtenerTodos();
		List<Finanza> ObtenerPorFecha(DateTime desde, DateTime hasta);
		(List<Finanza> Items, int Total) ObtenerPaginado(int mes, int anio, int pagina, int porPagina);
		Finanza? ObtenerPorId(int id);
		void Guardar(Finanza finanza);
		void Actualizar(Finanza finanza);
		void Eliminar(int id);
	}
}
