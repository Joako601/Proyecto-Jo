using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Infrastructure.Persistence.EfCore.Configurations
{
	public class ItemConfiguration : IEntityTypeConfiguration<Item>
	{
		public void Configure(EntityTypeBuilder<Item> builder)
		{
			builder.ToTable("items");
			builder.HasKey(i => i.Id);
			builder.Property(i => i.Platillo).IsRequired().HasMaxLength(200);
			builder.Property(i => i.Categoria).IsRequired().HasMaxLength(100);
			builder.Property(i => i.Precio).HasPrecision(18, 2);
		}
	}
}
