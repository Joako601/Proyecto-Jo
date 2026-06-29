using ProyectoJo.Domain.Entities;
using ProyectoJo.Infrastructure.Persistence;
using Xunit;

namespace ProyectoJo.Application.Tests.Infrastructure
{
	public class JsonFinanzaRepositoryConcurrencyTests : IDisposable
	{
		private readonly string _rutaArchivo;
		private readonly JsonFinanzaRepository _repository;

		public JsonFinanzaRepositoryConcurrencyTests()
		{
			_rutaArchivo = Path.Combine(Path.GetTempPath(), $"finanzas_test_{Guid.NewGuid()}.json");
			_repository = new JsonFinanzaRepository(_rutaArchivo);
		}

		public void Dispose()
		{
			if (File.Exists(_rutaArchivo)) File.Delete(_rutaArchivo);
		}

		[Fact]
		public void Guardar_ConEscriturasConcurrentes_NoGeneraIdsDuplicados()
		{
			// Arrange: 50 movimientos "creados" casi al mismo tiempo,
			// simulando Cocina/Recepción/Admin registrando ventas en simultáneo.
			const int cantidadConcurrente = 50;
			var tareas = new List<Task>();

			for (int i = 0; i < cantidadConcurrente; i++)
			{
				tareas.Add(Task.Run(() =>
				{
					_repository.Guardar(new Finanza
					{
						Monto = 10,
						Categoria = "Ventas",
						Descripcion = "Movimiento concurrente",
						Tipo = TipoMovimiento.Ingreso,
						Fecha = DateTime.Now
					});
				}));
			}

			// Act: todas las escrituras corren en paralelo de verdad
			Task.WaitAll(tareas.ToArray());

			// Assert: cada movimiento guardado debe tener un Id único
			var todos = _repository.ObtenerTodos();
			var idsUnicos = todos.Select(f => f.Id).Distinct().Count();

			Assert.Equal(cantidadConcurrente, todos.Count);
			Assert.Equal(cantidadConcurrente, idsUnicos);
		}
	}
}