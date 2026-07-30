using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Infrastructure.Persistence.EfCore
{
	public class ProyectoJoDbContext : DbContext
	{
		private static readonly ValueConverter<DateTime, DateTime> UtcDateTimeConverter =
			new(v => EnsureUtc(v), v => v);

		private static readonly ValueConverter<DateTime?, DateTime?> UtcNullableDateTimeConverter =
			new(v => v.HasValue ? EnsureUtc(v.Value) : v, v => v);

		public ProyectoJoDbContext(DbContextOptions<ProyectoJoDbContext> options) : base(options)
		{
		}

		public DbSet<Item> Items => Set<Item>();
		public DbSet<Finanza> Finanzas => Set<Finanza>();
		public DbSet<Pedido> Pedidos => Set<Pedido>();
		public DbSet<Promocion> Promociones => Set<Promocion>();
		public DbSet<Empleado> Empleados => Set<Empleado>();
		public DbSet<DispositivoOperaciones> Dispositivos => Set<DispositivoOperaciones>();
		public DbSet<CierreCaja> CierresCaja => Set<CierreCaja>();
		public DbSet<RegistroAuditoria> RegistrosAuditoria => Set<RegistroAuditoria>();
		public DbSet<Insumo> Insumos => Set<Insumo>();
		public DbSet<Receta> Recetas => Set<Receta>();
		public DbSet<OpinionCliente> Opiniones => Set<OpinionCliente>();
		public DbSet<Administrador> Administradores => Set<Administrador>();
		public DbSet<SupervisorClave> SupervisorClave => Set<SupervisorClave>();

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProyectoJoDbContext).Assembly);

			foreach (var entityType in modelBuilder.Model.GetEntityTypes())
			{
				foreach (var property in entityType.GetProperties())
				{
					if (property.ClrType == typeof(DateTime))
						property.SetValueConverter(UtcDateTimeConverter);
					else if (property.ClrType == typeof(DateTime?))
						property.SetValueConverter(UtcNullableDateTimeConverter);
				}
			}
		}

		private static DateTime EnsureUtc(DateTime value) => value.Kind switch
		{
			DateTimeKind.Utc => value,
			DateTimeKind.Local => value.ToUniversalTime(),
			_ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
		};
	}
}
