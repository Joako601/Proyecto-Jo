using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Infrastructure.Persistence.EfCore.Configurations
{
	public class CierreCajaConfiguration : IEntityTypeConfiguration<CierreCaja>
	{
		public void Configure(EntityTypeBuilder<CierreCaja> builder)
		{
			builder.ToTable("cierres_caja");
			builder.HasKey(c => c.Id);
			builder.Property(c => c.Estado).HasConversion<string>().HasMaxLength(20);
			builder.Property(c => c.FondoInicial).HasPrecision(18, 2);
			builder.Property(c => c.VentasDelDia).HasPrecision(18, 2);
			builder.Property(c => c.GastosDelDia).HasPrecision(18, 2);
			builder.Ignore(c => c.Total);
		}
	}
}
