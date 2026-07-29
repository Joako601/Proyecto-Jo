using System.Security.Cryptography;
using ProyectoJo.Application.Ports.In;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Application.UseCases
{
	public class EmpleadoAuthUseCase : IEmpleadoAuthService
	{
		private readonly IEmpleadoRepository _repository;

		private const int SaltSize = 16;
		private const int HashSize = 32;
		private const int Iteraciones = 100_000;

		public EmpleadoAuthUseCase(IEmpleadoRepository repository)
		{
			_repository = repository;
		}

		public async Task<Empleado?> ValidarCredencialesAsync(string nombre, string clave, RolEmpleado estacion)
		{
			if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(clave))
				return null;

			var empleados = await _repository.ObtenerTodosAsync();

			var empleado = empleados.FirstOrDefault(e =>
				e.Activo &&
				e.Rol == estacion &&
				string.Equals(e.Nombre, nombre.Trim(), StringComparison.OrdinalIgnoreCase));

			if (empleado is null) return null;

			return VerificarClave(clave, empleado.ClaveHash) ? empleado : null;
		}

		public static string HashearClave(string clave)
		{
			var salt = RandomNumberGenerator.GetBytes(SaltSize);
			var hash = Rfc2898DeriveBytes.Pbkdf2(clave, salt, Iteraciones, HashAlgorithmName.SHA256, HashSize);
			return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
		}

		private static bool VerificarClave(string claveIngresada, string claveHashGuardada)
		{
			var partes = claveHashGuardada.Split('.');
			if (partes.Length != 2) return false;

			var salt = Convert.FromBase64String(partes[0]);
			var hashGuardado = Convert.FromBase64String(partes[1]);

			var hashIngresado = Rfc2898DeriveBytes.Pbkdf2(claveIngresada, salt, Iteraciones, HashAlgorithmName.SHA256, HashSize);

			return CryptographicOperations.FixedTimeEquals(hashIngresado, hashGuardado);
		}
	}
}