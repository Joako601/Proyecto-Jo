using ProyectoJo.Application.DTOs;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Application.Ports.In
{
	public interface IRecetaService
	{
		List<Receta> ObtenerTodas();
		Receta? ObtenerPorId(int id);
		Receta? ObtenerPorItemId(int itemId);
		void Agregar(Receta receta, string usuario);
		bool Editar(Receta receta, string usuario);
		bool Eliminar(int id, string usuario);
		RendimientoRecetaDto? ObtenerRendimiento(int recetaId);
		List<RendimientoRecetaDto> ObtenerRendimientoDeTodas();
	}
}