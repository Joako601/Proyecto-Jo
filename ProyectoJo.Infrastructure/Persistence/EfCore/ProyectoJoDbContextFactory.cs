using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ProyectoJo.Infrastructure.Persistence.EfCore
{
	public class ProyectoJoDbContextFactory : IDesignTimeDbContextFactory<ProyectoJoDbContext>
	{
		public ProyectoJoDbContext CreateDbContext(string[] args)
		{
			var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Default")
				?? "Host=localhost;Port=5432;Database=proyectojo;Username=postgres;Password=postgres";

			var optionsBuilder = new DbContextOptionsBuilder<ProyectoJoDbContext>()
				.UseNpgsql(connectionString)
				.UseSnakeCaseNamingConvention();

			return new ProyectoJoDbContext(optionsBuilder.Options);
		}
	}
}
