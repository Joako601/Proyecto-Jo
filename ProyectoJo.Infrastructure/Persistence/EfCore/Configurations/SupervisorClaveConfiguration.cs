using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ProyectoJo.Infrastructure.Persistence.EfCore.Configurations
{
	public class SupervisorClaveConfiguration : IEntityTypeConfiguration<SupervisorClave>
	{
		public void Configure(EntityTypeBuilder<SupervisorClave> builder)
		{
			builder.ToTable("supervisor_clave");
			builder.HasKey(s => s.Id);
			builder.Property(s => s.ClaveHash).IsRequired().HasMaxLength(200);
		}
	}
}
