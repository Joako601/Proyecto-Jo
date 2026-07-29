namespace ProyectoJo.Application.Ports.In
{
	public record ResultadoAuth(string Usuario, string Rol);

	public interface IAuthService
	{
		Task<ResultadoAuth?> ValidarCredencialesAsync(string usuario, string contrasena);
	}
}