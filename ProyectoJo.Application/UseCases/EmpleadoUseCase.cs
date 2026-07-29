using ProyectoJo.Application.Ports.In;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Application.UseCases
{
	public class EmpleadoUseCase : IEmpleadoService
	{
		private readonly IEmpleadoRepository _repository;

		public EmpleadoUseCase(IEmpleadoRepository repository)
		{
			_repository = repository;
		}

		public Task<List<Empleado>> ObtenerTodosAsync() => _repository.ObtenerTodosAsync();

		public Task<Empleado?> ObtenerPorIdAsync(int id) => _repository.ObtenerPorIdAsync(id);

		public async Task<(bool Exito, string? Error)> CrearAsync(string nombre, string pin, RolEmpleado rol)
		{
			if (string.IsNullOrWhiteSpace(nombre))
				return (false, "El nombre es obligatorio.");

			if (string.IsNullOrWhiteSpace(pin) || pin.Length < 4)
				return (false, "El PIN debe tener al menos 4 dígitos.");

			var empleado = new Empleado
			{
				Nombre = nombre.Trim(),
				PinHash = EmpleadoAuthUseCase.HashearPin(pin),
				Rol = rol,
				Activo = true
			};

			await _repository.AgregarAsync(empleado);
			return (true, null);
		}

		public async Task<(bool Exito, string? Error)> EditarAsync(int id, string nombre, bool activo, RolEmpleado rol, string? nuevoPin)
		{
			var empleado = await _repository.ObtenerPorIdAsync(id);
			if (empleado is null)
				return (false, "El operador no existe.");

			if (string.IsNullOrWhiteSpace(nombre))
				return (false, "El nombre es obligatorio.");

			empleado.Nombre = nombre.Trim();
			empleado.Activo = activo;
			empleado.Rol = rol;

			if (!string.IsNullOrWhiteSpace(nuevoPin))
			{
				if (nuevoPin.Length < 4)
					return (false, "El nuevo PIN debe tener al menos 4 dígitos.");
				empleado.PinHash = EmpleadoAuthUseCase.HashearPin(nuevoPin);
			}

			var actualizado = await _repository.ActualizarAsync(empleado);
			return actualizado ? (true, null) : (false, "No se pudo actualizar el operador.");
		}

		public Task<bool> EliminarAsync(int id) => _repository.EliminarAsync(id);
	}
}