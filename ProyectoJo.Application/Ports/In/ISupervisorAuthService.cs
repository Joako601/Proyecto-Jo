namespace ProyectoJo.Application.Ports.In
{
	public interface ISupervisorAuthService
	{
		Task<bool> ValidarClaveAsync(string clave);
		Task<bool> TieneClaveConfiguradaAsync();
		Task<bool> CambiarClaveAsync(string? claveActual, string claveNueva);
	}
}