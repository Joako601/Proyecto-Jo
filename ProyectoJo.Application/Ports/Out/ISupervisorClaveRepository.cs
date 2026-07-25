namespace ProyectoJo.Application.Ports.Out
{
	public interface ISupervisorClaveRepository
	{
		Task<string?> ObtenerHashAsync();
		Task GuardarHashAsync(string hash);
	}
}