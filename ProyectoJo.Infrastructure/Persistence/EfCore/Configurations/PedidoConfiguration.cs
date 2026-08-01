using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Infrastructure.Persistence.EfCore.Configurations
{
	public class PedidoConfiguration : IEntityTypeConfiguration<Pedido>
	{
		public void Configure(EntityTypeBuilder<Pedido> builder)
		{
			builder.ToTable("pedidos");
			builder.HasKey(p => p.Id);
			builder.Property(p => p.Mesa).IsRequired().HasMaxLength(50);
			builder.Property(p => p.Estado).HasConversion<string>().HasMaxLength(20);
			builder.Ignore(p => p.Total);
			builder.HasIndex(p => new { p.Estado, p.FechaCreacion });

			builder.OwnsMany(p => p.Items, items =>
			{
				items.ToTable("pedido_items");
				items.WithOwner().HasForeignKey("PedidoId");
				items.Property<int>("Id");
				items.HasKey("Id");
				items.Property(i => i.Nombre).IsRequired().HasMaxLength(200);
				items.Property(i => i.PrecioUnitario).HasPrecision(18, 2);
				items.Ignore(i => i.Subtotal);
			});
		}
	}
}
