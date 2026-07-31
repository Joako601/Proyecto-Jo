namespace ProyectoJo.Application.Ports.In
{
	public interface ISupervisorAuthService
	{
		Task<bool> ValidarClaveAsync(string clave);
	}
}
