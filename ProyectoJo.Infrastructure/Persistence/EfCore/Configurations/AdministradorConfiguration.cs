using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Infrastructure.Persistence.EfCore.Configurations
{
	public class AdministradorConfiguration : IEntityTypeConfiguration<Administrador>
	{
		public void Configure(EntityTypeBuilder<Administrador> builder)
		{
			builder.ToTable("administradores");
			builder.HasKey(a => a.Id);
			builder.Property(a => a.Usuario).IsRequired().HasMaxLength(100);
			builder.HasIndex(a => a.Usuario).IsUnique();
			builder.Property(a => a.ContrasenaHash).IsRequired().HasMaxLength(200);
			builder.Property(a => a.ClaveSupervisorHash).HasMaxLength(200);
		}
	}
}
