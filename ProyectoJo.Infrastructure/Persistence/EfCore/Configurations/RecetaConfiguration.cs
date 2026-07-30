using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Infrastructure.Persistence.EfCore.Configurations
{
	public class RecetaConfiguration : IEntityTypeConfiguration<Receta>
	{
		public void Configure(EntityTypeBuilder<Receta> builder)
		{
			builder.ToTable("recetas");
			builder.HasKey(r => r.Id);
			builder.Property(r => r.NombreReceta).IsRequired().HasMaxLength(200);
			builder.Property(r => r.UnidadRendimiento).IsRequired().HasMaxLength(50);
			builder.Ignore(r => r.CostoTotal);
			builder.Ignore(r => r.CostoPorPorcion);

			builder.OwnsMany(r => r.Ingredientes, ingredientes =>
			{
				ingredientes.ToTable("receta_ingredientes");
				ingredientes.WithOwner().HasForeignKey("RecetaId");
				ingredientes.Property<int>("Id");
				ingredientes.HasKey("Id");
				ingredientes.Property(i => i.Nombre).IsRequired().HasMaxLength(200);
				ingredientes.Property(i => i.Unidad).HasConversion<string>().HasMaxLength(20);
				ingredientes.Property(i => i.Cantidad).HasPrecision(18, 4);
				ingredientes.Property(i => i.CostoUnitario).HasPrecision(18, 4);
				ingredientes.Ignore(i => i.CostoTotal);
			});
		}
	}
}
