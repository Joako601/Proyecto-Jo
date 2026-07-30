using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Infrastructure.Persistence.EfCore.Configurations
{
	public class DispositivoConfiguration : IEntityTypeConfiguration<DispositivoOperaciones>
	{
		public void Configure(EntityTypeBuilder<DispositivoOperaciones> builder)
		{
			builder.ToTable("dispositivos_operaciones");
			builder.HasKey(d => d.Id);
			builder.Property(d => d.Token).IsRequired().HasMaxLength(100);
			builder.HasIndex(d => d.Token).IsUnique();
			builder.Property(d => d.Nombre).IsRequired(false).HasMaxLength(200);
			builder.Property(d => d.Estacion).HasConversion<string>().HasMaxLength(20);
		}
	}
}
