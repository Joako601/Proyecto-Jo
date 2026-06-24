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

		public async Task<Empleado?> ValidarPinAsync(string pin, RolEmpleado estacion)
		{
			if (string.IsNullOrWhiteSpace(pin)) return null;

			var empleados = await _repository.ObtenerTodosAsync();

			foreach (var empleado in empleados.Where(e => e.Activo && e.Rol == estacion))
			{
				if (VerificarPin(pin, empleado.PinHash))
					return empleado;
			}

			return null;
		}

		public static string HashearPin(string pin)
		{
			var salt = RandomNumberGenerator.GetBytes(SaltSize);
			var hash = Rfc2898DeriveBytes.Pbkdf2(pin, salt, Iteraciones, HashAlgorithmName.SHA256, HashSize);
			return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
		}

		private static bool VerificarPin(string pinIngresado, string pinHashGuardado)
		{
			var partes = pinHashGuardado.Split('.');
			if (partes.Length != 2) return false;

			var salt = Convert.FromBase64String(partes[0]);
			var hashGuardado = Convert.FromBase64String(partes[1]);

			var hashIngresado = Rfc2898DeriveBytes.Pbkdf2(pinIngresado, salt, Iteraciones, HashAlgorithmName.SHA256, HashSize);

			return CryptographicOperations.FixedTimeEquals(hashIngresado, hashGuardado);
		}
	}
}