using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Infrastructure.Persistence.EfCore.Configurations
{
	public class OpinionConfiguration : IEntityTypeConfiguration<OpinionCliente>
	{
		public void Configure(EntityTypeBuilder<OpinionCliente> builder)
		{
			builder.ToTable("opiniones");
			builder.HasKey(o => o.Id);
			builder.Property(o => o.NombreCliente).HasMaxLength(200);
			builder.Property(o => o.Comentario).IsRequired().HasMaxLength(1000);
			builder.Property(o => o.Calificacion).HasPrecision(3, 1);
			builder.Property(o => o.Estado).HasConversion<string>().HasMaxLength(20);
			builder.Property(o => o.RegistradoPor).IsRequired().HasMaxLength(200);
		}
	}
}
