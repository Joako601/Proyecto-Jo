using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using ProyectoJo.Domain.Entities;

namespace ProyectoJo.Infrastructure.Persistence.EfCore
{
	public static class JsonToPostgresSeeder
	{
		private static readonly string[] Tablas =
		{
			"items", "finanzas", "promociones", "empleados", "dispositivos_operaciones",
			"cierres_caja", "auditoria", "insumos", "recetas", "opiniones",
			"administradores", "pedidos", "supervisor_clave"
		};

		private static readonly JsonSerializerOptions Plano = new();

		private static readonly JsonSerializerOptions ConEnumsComoTexto = new()
		{
			Converters = { new JsonStringEnumConverter() }
		};

		public static async Task<bool> SeedAsync(ProyectoJoDbContext context, string rutaPersistencia)
		{
			if (await HayDatosExistentesAsync(context))
			{
				Console.WriteLine("La base de datos ya tiene datos. No se importó nada (borrá las tablas o usá otra base si querés reimportar).");
				return false;
			}

			await ImportarAsync(context, context.Items, rutaPersistencia, "menu.json", Plano);
			await ImportarAsync(context, context.Finanzas, rutaPersistencia, "finanzas.json", Plano);
			await ImportarAsync(context, context.Promociones, rutaPersistencia, "promociones.json", Plano);
			await ImportarAsync(context, context.Empleados, rutaPersistencia, "empleados.json", ConEnumsComoTexto);
			await ImportarAsync(context, context.Dispositivos, rutaPersistencia, "dispositivos.json", ConEnumsComoTexto, preservarId: false);
			await ImportarAsync(context, context.CierresCaja, rutaPersistencia, "cierres-caja.json", Plano);
			await ImportarAsync(context, context.RegistrosAuditoria, rutaPersistencia, "auditoria.json", Plano);
			await ImportarAsync(context, context.Insumos, rutaPersistencia, "insumos.json", ConEnumsComoTexto);
			await ImportarAsync(context, context.Recetas, rutaPersistencia, "recetas.json", Plano);
			await ImportarAsync(context, context.Opiniones, rutaPersistencia, "opiniones.json", Plano);
			await ImportarAsync(context, context.Administradores, rutaPersistencia, "administradores.json", Plano);
			await ImportarAsync(context, context.Pedidos, rutaPersistencia, "pedidos.json", ConEnumsComoTexto);
			await ImportarSupervisorClaveAsync(context, rutaPersistencia);

			await ReiniciarSecuenciasAsync(context);

			Console.WriteLine("Importación completa.");
			return true;
		}

		private static async Task<bool> HayDatosExistentesAsync(ProyectoJoDbContext context) =>
			await context.Items.AnyAsync() ||
			await context.Finanzas.AnyAsync() ||
			await context.Promociones.AnyAsync() ||
			await context.Empleados.AnyAsync() ||
			await context.Dispositivos.AnyAsync() ||
			await context.CierresCaja.AnyAsync() ||
			await context.RegistrosAuditoria.AnyAsync() ||
			await context.Insumos.AnyAsync() ||
			await context.Recetas.AnyAsync() ||
			await context.Opiniones.AnyAsync() ||
			await context.Administradores.AnyAsync() ||
			await context.Pedidos.AnyAsync() ||
			await context.SupervisorClave.AnyAsync();

		private static async Task ImportarAsync<T>(
			ProyectoJoDbContext context,
			DbSet<T> dbSet,
			string rutaPersistencia,
			string nombreArchivo,
			JsonSerializerOptions opciones,
			bool preservarId = true) where T : class
		{
			var rutaArchivo = Path.Combine(rutaPersistencia, nombreArchivo);
			if (!File.Exists(rutaArchivo))
			{
				Console.WriteLine($"{nombreArchivo}: no existe, se omite.");
				return;
			}

			var json = await File.ReadAllTextAsync(rutaArchivo);
			if (string.IsNullOrWhiteSpace(json))
			{
				Console.WriteLine($"{nombreArchivo}: vacío, se omite.");
				return;
			}

			var registros = JsonSerializer.Deserialize<List<T>>(json, opciones) ?? new List<T>();
			if (registros.Count == 0)
			{
				Console.WriteLine($"{nombreArchivo}: sin registros.");
				return;
			}

			if (!preservarId)
			{
				var idProperty = typeof(T).GetProperty("Id");
				foreach (var registro in registros)
					idProperty?.SetValue(registro, 0);
			}

			dbSet.AddRange(registros);
			await context.SaveChangesAsync();
			Console.WriteLine($"{nombreArchivo}: {registros.Count} registro(s) importado(s).");
		}

		private static async Task ImportarSupervisorClaveAsync(ProyectoJoDbContext context, string rutaPersistencia)
		{
			var rutaArchivo = Path.Combine(rutaPersistencia, "supervisor-clave.json");
			if (!File.Exists(rutaArchivo))
			{
				Console.WriteLine("supervisor-clave.json: no existe, se omite.");
				return;
			}

			var json = await File.ReadAllTextAsync(rutaArchivo);
			if (string.IsNullOrWhiteSpace(json))
			{
				Console.WriteLine("supervisor-clave.json: vacío, se omite.");
				return;
			}

			var dto = JsonSerializer.Deserialize<SupervisorClaveDto>(json, Plano);
			if (string.IsNullOrWhiteSpace(dto?.ClaveHash))
			{
				Console.WriteLine("supervisor-clave.json: sin hash, se omite.");
				return;
			}

			context.SupervisorClave.Add(new SupervisorClave { Id = 1, ClaveHash = dto.ClaveHash });
			await context.SaveChangesAsync();
			Console.WriteLine("supervisor-clave.json: clave importada.");
		}

		private static async Task ReiniciarSecuenciasAsync(ProyectoJoDbContext context)
		{
			foreach (var tabla in Tablas)
			{
#pragma warning disable EF1002
				await context.Database.ExecuteSqlRawAsync(
					$"SELECT setval(pg_get_serial_sequence('{tabla}', 'id'), COALESCE((SELECT MAX(id) FROM {tabla}), 0) + 1, false);");
#pragma warning restore EF1002
			}
		}

		public static async Task ResetAsync(ProyectoJoDbContext context)
		{
#pragma warning disable EF1002
			await context.Database.ExecuteSqlRawAsync(
				$"TRUNCATE TABLE {string.Join(", ", Tablas)} RESTART IDENTITY CASCADE;");
#pragma warning restore EF1002
			Console.WriteLine("Todas las tablas quedaron vacías.");
		}

		private class SupervisorClaveDto
		{
			public string ClaveHash { get; set; } = string.Empty;
		}
	}
}
