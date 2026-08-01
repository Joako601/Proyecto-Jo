using ProyectoJo.Application.Ports.In;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Application.UseCases
{
	public class EmpleadoUseCase : IEmpleadoService
	{
		private readonly IEmpleadoRepository _repository;

		private const int ClaveMinima = 6;

		public EmpleadoUseCase(IEmpleadoRepository repository)
		{
			_repository = repository;
		}

		public Task<List<Empleado>> ObtenerTodosAsync() => _repository.ObtenerTodosAsync();

		public Task<Empleado?> ObtenerPorIdAsync(int id) => _repository.ObtenerPorIdAsync(id);

		public async Task<(bool Exito, string? Error)> CrearAsync(string nombre, string clave, RolEmpleado rol)
		{
			if (string.IsNullOrWhiteSpace(nombre))
				return (false, "El nombre es obligatorio.");

			if (string.IsNullOrWhiteSpace(clave) || clave.Length < ClaveMinima)
				return (false, $"La clave debe tener al menos {ClaveMinima} caracteres.");

			var nombreNormalizado = nombre.Trim();
			var existentes = await _repository.ObtenerTodosAsync();
			var duplicado = existentes.Any(e =>
				e.Rol == rol && string.Equals(e.Nombre, nombreNormalizado, StringComparison.OrdinalIgnoreCase));

			if (duplicado)
				return (false, "Ya existe un operador con ese nombre en esa estación.");

			var empleado = new Empleado
			{
				Nombre = nombreNormalizado,
				ClaveHash = EmpleadoAuthUseCase.HashearClave(clave),
				Rol = rol,
				Activo = true
			};

			await _repository.AgregarAsync(empleado);
			return (true, null);
		}

		public async Task<(bool Exito, string? Error)> EditarAsync(int id, string nombre, bool activo, RolEmpleado rol, string? nuevaClave)
		{
			var empleado = await _repository.ObtenerPorIdAsync(id);
			if (empleado is null)
				return (false, "El operador no existe.");

			if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(nuevaClave))
				return (false, "El nombre y la clave son obligatorios.");

			if (nuevaClave.Length < ClaveMinima)
				return (false, $"La nueva clave debe tener al menos {ClaveMinima} caracteres.");

			var nombreNormalizado = nombre.Trim();
			var existentes = await _repository.ObtenerTodosAsync();
			var duplicado = existentes.Any(e =>
				e.Id != id && e.Rol == rol && string.Equals(e.Nombre, nombreNormalizado, StringComparison.OrdinalIgnoreCase));

			if (duplicado)
				return (false, "Ya existe un operador con ese nombre en esa estación.");

			empleado.Nombre = nombreNormalizado;
			empleado.Activo = activo;
			empleado.Rol = rol;
			empleado.ClaveHash = EmpleadoAuthUseCase.HashearClave(nuevaClave);

			var actualizado = await _repository.ActualizarAsync(empleado);
			return actualizado ? (true, null) : (false, "No se pudo actualizar el operador.");
		}

		public Task<bool> EliminarAsync(int id) => _repository.EliminarAsync(id);
	}
}