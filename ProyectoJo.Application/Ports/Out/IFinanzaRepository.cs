using ProyectoJo.Domain.Entities;


namespace ProyectoJo.Application.Ports.Out
{
	public interface IFinanzaRepository
	{
		List<Finanza> ObtenerTodos();
		Finanza? ObtenerPorId(int id);
		void Guardar(Finanza finanza);
		void Actualizar(Finanza finanza);
		void Eliminar(int id);
	}
}
