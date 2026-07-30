using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Infrastructure.Persistence.EfCore.Configurations
{
	public class RegistroAuditoriaConfiguration : IEntityTypeConfiguration<RegistroAuditoria>
	{
		public void Configure(EntityTypeBuilder<RegistroAuditoria> builder)
		{
			builder.ToTable("auditoria");
			builder.HasKey(r => r.Id);
			builder.Property(r => r.Usuario).IsRequired().HasMaxLength(200);
			builder.Property(r => r.Modulo).IsRequired().HasMaxLength(100);
			builder.Property(r => r.Accion).HasConversion<string>().HasMaxLength(20);
			builder.Property(r => r.Entidad).IsRequired().HasMaxLength(300);
		}
	}
}
