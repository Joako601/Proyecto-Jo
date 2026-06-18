using ProyectoJo.Application.Ports.In;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Application.UseCases
{
	public class ProductoUseCase : IProductoService
	{
		private readonly IProductoRepository _repository;

		public ProductoUseCase(IProductoRepository repository)
		{
			_repository = repository;
		}

		public IEnumerable<Item> ObtenerTodos()
		{
			return _repository.ObtenerTodos();
		}

		public IEnumerable<Item> ObtenerPorCategoria(string categoria)
		{
			return _repository.ObtenerPorCategoria(categoria);
		}
	}
}