using ProyectoJo.Domain.Entities;

public interface ICierreCajaRepository
{
	List<CierreCaja> ObtenerTodos();
	CierreCaja? ObtenerPorId(int id);
	void Guardar(CierreCaja cierreCaja);
	void Actualizar(CierreCaja cierreCaja);
	bool IntentarAbrir(CierreCaja nuevaCaja);
}