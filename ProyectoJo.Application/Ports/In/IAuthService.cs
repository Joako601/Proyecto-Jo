namespace ProyectoJo.Application.Ports.In
{
	public record ResultadoAuth(string Usuario, string Rol, List<string> Areas);

	public interface IAuthService
	{
		Task<ResultadoAuth?> ValidarCredencialesAsync(string usuario, string contrasena);
	}
}