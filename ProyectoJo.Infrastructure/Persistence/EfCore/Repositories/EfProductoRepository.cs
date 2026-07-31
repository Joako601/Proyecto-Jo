using Microsoft.EntityFrameworkCore;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Infrastructure.Persistence.EfCore.Repositories
{
	public class EfProductoRepository : IProductoRepository
	{
		private readonly ProyectoJoDbContext _context;

		public EfProductoRepository(ProyectoJoDbContext context)
		{
			_context = context;
		}

		public IEnumerable<Item> ObtenerTodos() => _context.Items.AsNoTracking().OrderBy(i => i.Id).ToList();

		public IEnumerable<Item> ObtenerPorCategoria(string categoria) =>
			_context.Items.AsNoTracking().Where(i => i.Categoria == categoria).OrderBy(i => i.Id).ToList();

		public List<Item> ObtenerMenu() => _context.Items.AsNoTracking().OrderBy(i => i.Id).ToList();

		public void ActualizarItem(Item item)
		{
			_context.Items.Update(item);
			_context.SaveChanges();
		}

		public void AgregarItem(Item item)
		{
			_context.Items.Add(item);
			_context.SaveChanges();
		}

		public Item? ObtenerPorId(int id) => _context.Items.AsNoTracking().FirstOrDefault(i => i.Id == id);

		public bool Eliminar(int id)
		{
			var item = _context.Items.Find(id);
			if (item is null) return false;

			_context.Items.Remove(item);
			_context.SaveChanges();
			return true;
		}

		public bool ToggleActivo(int id)
		{
			var item = _context.Items.Find(id);
			if (item is null) return false;

			item.Activo = !item.Activo;
			_context.SaveChanges();
			return true;
		}

		public bool ToggleAgotado(int id)
		{
			var item = _context.Items.Find(id);
			if (item is null) return false;

			item.Agotado = !item.Agotado;
			_context.SaveChanges();
			return true;
		}
	}
}
