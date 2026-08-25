using ProyectoJo.Domain.Entities;

public interface ICierreCajaRepository
{
	List<CierreCaja> ObtenerTodos();
	CierreCaja? ObtenerPorId(int id);
	void Guardar(CierreCaja cierreCaja);
	void Actualizar(CierreCaja cierreCaja);
	bool IntentarAbrir(CierreCaja nuevaCaja);
	(CierreCaja? Caja, string? Error) CerrarAtomico(int id, Func<CierreCaja, string?> aplicarCierre);
}