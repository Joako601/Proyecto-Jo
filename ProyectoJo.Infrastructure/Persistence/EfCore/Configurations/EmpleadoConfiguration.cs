using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Infrastructure.Persistence.EfCore.Configurations
{
	public class EmpleadoConfiguration : IEntityTypeConfiguration<Empleado>
	{
		public void Configure(EntityTypeBuilder<Empleado> builder)
		{
			builder.ToTable("empleados");
			builder.HasKey(e => e.Id);
			builder.Property(e => e.Nombre).IsRequired().HasMaxLength(200);
			builder.Property(e => e.ClaveHash).IsRequired().HasMaxLength(200);
			builder.Property(e => e.Rol).HasConversion<string>().HasMaxLength(20);
		}
	}
}
