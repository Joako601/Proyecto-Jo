using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Application.Ports.Out
{
	public interface IInsumoRepository
	{
		List<Insumo> ObtenerTodos();
		Insumo? ObtenerPorId(int id);
		void Agregar(Insumo insumo);
		void AgregarRango(IEnumerable<Insumo> insumos);
		bool Editar(Insumo insumo);
		bool Eliminar(int id);
		Task<(bool Exitoso, List<FaltanteInsumo> Faltantes)> DescontarAtomicoAsync(Dictionary<int, decimal> consumoPorInsumoId);

		Task<Insumo?> ReponerAtomicoAsync(int id, decimal cantidad);
	}

	public class FaltanteInsumo
	{
		public int InsumoId { get; set; }
		public string Nombre { get; set; } = string.Empty;
		public decimal Necesario { get; set; }
		public decimal Disponible { get; set; }
	}
}