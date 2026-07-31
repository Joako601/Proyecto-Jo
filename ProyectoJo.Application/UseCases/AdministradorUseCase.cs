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

		public async Task<(bool Exito, string? Error)> CrearAsync(string usuario, string contrasena, List<string> areas, string? claveSupervisor)
		{
			if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(contrasena) || string.IsNullOrWhiteSpace(claveSupervisor))
				return (false, "Usuario, contraseña y clave de supervisor son obligatorios.");

			if (contrasena.Length < 8)
				return (false, "La contraseña debe tener al menos 8 caracteres.");

			if (claveSupervisor.Length < 6)
				return (false, "La clave de supervisor debe tener al menos 6 caracteres.");

			var existente = await _repository.ObtenerPorUsuarioAsync(usuario.Trim());
			if (existente is not null)
				return (false, "Ya existe un administrador con ese usuario.");

			var administrador = new Administrador
			{
				Usuario = usuario.Trim(),
				ContrasenaHash = HashearContrasena(contrasena),
				ClaveSupervisorHash = HashearContrasena(claveSupervisor),
				Activo = true,
				Areas = NormalizarAreas(areas)
			};

			await _repository.AgregarAsync(administrador);
			return (true, null);
		}

		public async Task<(bool Exito, string? Error)> EditarAsync(int id, string usuario, bool activo, string? nuevaContrasena, List<string> areas, string? nuevaClaveSupervisor = null)
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
			administrador.Areas = NormalizarAreas(areas);

			if (!string.IsNullOrWhiteSpace(nuevaContrasena))
			{
				if (nuevaContrasena.Length < 8)
					return (false, "La nueva contraseña debe tener al menos 8 caracteres.");
				administrador.ContrasenaHash = HashearContrasena(nuevaContrasena);
			}

			if (!string.IsNullOrWhiteSpace(nuevaClaveSupervisor))
			{
				if (nuevaClaveSupervisor.Length < 6)
					return (false, "La nueva clave de supervisor debe tener al menos 6 caracteres.");
				administrador.ClaveSupervisorHash = HashearContrasena(nuevaClaveSupervisor);
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

		private static List<string> NormalizarAreas(List<string>? areas)
		{
			if (areas is null || areas.Count == 0) return new List<string>();
			return areas.Where(a => AreasAdmin.Todas.Contains(a)).Distinct().ToList();
		}
	}
}