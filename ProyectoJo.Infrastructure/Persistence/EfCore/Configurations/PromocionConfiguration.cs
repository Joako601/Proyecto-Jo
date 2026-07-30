using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Infrastructure.Persistence.EfCore.Configurations
{
	public class PromocionConfiguration : IEntityTypeConfiguration<Promocion>
	{
		public void Configure(EntityTypeBuilder<Promocion> builder)
		{
			builder.ToTable("promociones");
			builder.HasKey(p => p.Id);
			builder.Property(p => p.Titulo).IsRequired().HasMaxLength(200);
			builder.Property(p => p.TipoDescuento).HasConversion<string>().HasMaxLength(20);
			builder.Property(p => p.ValorDescuento).HasPrecision(18, 2);
		}
	}
}
