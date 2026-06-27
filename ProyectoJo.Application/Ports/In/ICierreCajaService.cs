using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Application.Ports.In
{
	public interface ICierreCajaService
	{
		// ¿Hay una caja abierta en este momento? (null si no hay ninguna)
		CierreCaja? ObtenerCajaAbierta();

		CierreCaja AbrirCaja(decimal fondoInicial, string? notas);

		CierreCaja CerrarCaja(int id, string? notas);

		// Calcula ventas/gastos del turno SIN guardar nada (para mostrar antes de confirmar el cierre)
		CierreCaja ObtenerVistaPreviaCierre(int id);

		List<CierreCaja> ObtenerHistorial();

		CierreCaja? ObtenerPorId(int id);
	}
}