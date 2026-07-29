using System.Security.Cryptography;
using ProyectoJo.Application.Ports.In;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Application.UseCases
{
	public class AdministradorUseCase : IAdministradorService
	{
		private readonly IAdministradorRepository _repository;

		private const int SaltSize = 16;
		private const int HashSize = 32;
		private const int Iteraciones = 100_000;

		public AdministradorUseCase(IAdministradorRepository repository)
		{
			_repository = repository;
		}

		public Task<List<Administrador>> ObtenerTodosAsync() => _repository.ObtenerTodosAsync();

		public Task<Administrador?> ObtenerPorIdAsync(int id) => _repository.ObtenerPorIdAsync(id);

		public async Task<(bool Exito, string? Error)> CrearAsync(string usuario, string contrasena)
		{
			if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(contrasena))
				return (false, "Usuario y contraseña son obligatorios.");

			if (contrasena.Length < 8)
				return (false, "La contraseña debe tener al menos 8 caracteres.");

			var existente = await _repository.ObtenerPorUsuarioAsync(usuario.Trim());
			if (existente is not null)
				return (false, "Ya existe un administrador con ese usuario.");

			var administrador = new Administrador
			{
				Usuario = usuario.Trim(),
				ContrasenaHash = HashearContrasena(contrasena),
				Activo = true
			};

			await _repository.AgregarAsync(administrador);
			return (true, null);
		}

		public async Task<(bool Exito, string? Error)> EditarAsync(int id, string usuario, bool activo, string? nuevaContrasena)
		{
			var administrador = await _repository.ObtenerPorIdAsync(id);
			if (administrador is null)
				return (false, "El administrador no existe.");

			if (string.IsNullOrWhiteSpace(usuario))
				return (false, "El usuario es obligatorio.");

			var duplicado = await _repository.ObtenerPorUsuarioAsync(usuario.Trim());
			if (duplicado is not null && duplicado.Id != id)
				return (false, "Ya existe un administrador con ese usuario.");

			administrador.Usuario = usuario.Trim();
			administrador.Activo = activo;

			if (!string.IsNullOrWhiteSpace(nuevaContrasena))
			{
				if (nuevaContrasena.Length < 8)
					return (false, "La nueva contraseña debe tener al menos 8 caracteres.");
				administrador.ContrasenaHash = HashearContrasena(nuevaContrasena);
			}

			var actualizado = await _repository.ActualizarAsync(administrador);
			return actualizado ? (true, null) : (false, "No se pudo actualizar el administrador.");
		}

		public Task<bool> EliminarAsync(int id) => _repository.EliminarAsync(id);

		public static string HashearContrasena(string contrasena)
		{
			var salt = RandomNumberGenerator.GetBytes(SaltSize);
			var hash = Rfc2898DeriveBytes.Pbkdf2(contrasena, salt, Iteraciones, HashAlgorithmName.SHA256, HashSize);
			return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
		}
	}
}