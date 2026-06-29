using ProyectoJo.Domain.Entities;
using ProyectoJo.Infrastructure.Persistence;
using Xunit;

namespace ProyectoJo.Application.Tests.Infrastructure
{
	public class JsonCierreCajaRepositoryConcurrencyTests : IDisposable
	{
		private readonly string _rutaArchivo;
		private readonly JsonCierreCajaRepository _repository;

		public JsonCierreCajaRepositoryConcurrencyTests()
		{
			_rutaArchivo = Path.Combine(Path.GetTempPath(), $"cierres_test_{Guid.NewGuid()}.json");
			_repository = new JsonCierreCajaRepository(_rutaArchivo);
		}

		public void Dispose()
		{
			if (File.Exists(_rutaArchivo)) File.Delete(_rutaArchivo);
		}

		[Fact]
		public void IntentarAbrir_ConLlamadasConcurrentes_SoloUnaTieneExito()
		{
			// Arrange: 20 intentos de abrir caja casi al mismo tiempo,
			// simulando dos empleados (o un doble clic) compitiendo por abrir turno.
			const int cantidadConcurrente = 20;
			var resultados = new bool[cantidadConcurrente];
			var tareas = new List<Task>();

			for (int i = 0; i < cantidadConcurrente; i++)
			{
				var indice = i;
				tareas.Add(Task.Run(() =>
				{
					var nuevaCaja = new CierreCaja
					{
						Estado = EstadoCaja.Abierta,
						FechaApertura = DateTime.Now,
						FondoInicial = 500
					};
					resultados[indice] = _repository.IntentarAbrir(nuevaCaja);
				}));
			}

			// Act: todas las llamadas corren en paralelo
			Task.WaitAll(tareas.ToArray());

			// Assert: exactamente una llamada debe haber tenido éxito
			var exitosas = resultados.Count(r => r);
			Assert.Equal(1, exitosas);

			// Y el archivo final debe reflejar una sola caja abierta, no más
			var todas = _repository.ObtenerTodos();
			var cajasAbiertas = todas.Count(c => c.Estado == EstadoCaja.Abierta);
			Assert.Equal(1, cajasAbiertas);
			Assert.Equal(1, todas.Count);
		}
	}
}