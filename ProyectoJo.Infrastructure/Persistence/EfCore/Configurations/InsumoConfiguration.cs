using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Infrastructure.Persistence.EfCore.Configurations
{
	public class InsumoConfiguration : IEntityTypeConfiguration<Insumo>
	{
		public void Configure(EntityTypeBuilder<Insumo> builder)
		{
			builder.ToTable("insumos");
			builder.HasKey(i => i.Id);
			builder.Property(i => i.Nombre).IsRequired().HasMaxLength(200);
			builder.Property(i => i.Unidad).HasConversion<string>().HasMaxLength(20);
			builder.Property(i => i.StockActual).HasPrecision(18, 4);
			builder.Property(i => i.StockMinimo).HasPrecision(18, 4);
			builder.Ignore(i => i.Agotado);
			builder.Ignore(i => i.StockBajo);
		}
	}
}
