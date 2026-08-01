using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Infrastructure.Persistence.EfCore.Configurations
{
	public class FinanzaConfiguration : IEntityTypeConfiguration<Finanza>
	{
		public void Configure(EntityTypeBuilder<Finanza> builder)
		{
			builder.ToTable("finanzas");
			builder.HasKey(f => f.Id);
			builder.Property(f => f.Tipo).HasConversion<string>().HasMaxLength(20);
			builder.Property(f => f.Categoria).IsRequired().HasMaxLength(100);
			builder.Property(f => f.Descripcion).IsRequired().HasMaxLength(500);
			builder.Property(f => f.Monto).HasPrecision(18, 2);
			builder.HasIndex(f => f.Fecha);
		}
	}
}
