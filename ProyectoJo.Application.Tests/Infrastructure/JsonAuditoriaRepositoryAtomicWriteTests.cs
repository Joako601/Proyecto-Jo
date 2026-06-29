using ProyectoJo.Domain.Entities;
using ProyectoJo.Infrastructure.Persistence;
using Xunit;

namespace ProyectoJo.Application.Tests.Infrastructure
{
	public class JsonAuditoriaRepositoryAtomicWriteTests : IDisposable
	{
		private readonly string _rutaArchivo;
		private readonly string _rutaTemporal;
		private readonly JsonAuditoriaRepository _repository;

		public JsonAuditoriaRepositoryAtomicWriteTests()
		{
			_rutaArchivo = Path.Combine(Path.GetTempPath(), $"auditoria_test_{Guid.NewGuid()}.json");
			_rutaTemporal = _rutaArchivo + ".tmp";
			_repository = new JsonAuditoriaRepository(_rutaArchivo);
		}

		public void Dispose()
		{
			if (File.Exists(_rutaArchivo)) File.Delete(_rutaArchivo);
			if (File.Exists(_rutaTemporal)) File.Delete(_rutaTemporal);
		}

		[Fact]
		public void Guardar_DespuesDeEscribir_NoDejaArchivoTemporalResidual()
		{
			// Arrange + Act
			_repository.Guardar(new RegistroAuditoria
			{
				Usuario = "admin",
				Modulo = "Productos",
				Accion = TipoAccionAuditoria.Creacion,
				Entidad = "Item de prueba",
				FechaHora = DateTime.Now
			});

			// Assert: la escritura exitosa termina en Move, el .tmp no debe sobrevivir
			Assert.False(File.Exists(_rutaTemporal));
			Assert.True(File.Exists(_rutaArchivo));
		}

		[Fact]
		public void Guardar_ConUnTmpResidualDeUnaEscrituraInterrumpidaPrevia_NoCorrompeElArchivoFinal()
		{
			// Arrange: simula que un proceso anterior murió justo después de
			// escribir el .tmp pero antes de hacer el Move
			File.WriteAllText(_rutaTemporal, "{ esto no es json valido, quedo a medio escribir");

			// Y el archivo "real" anterior, válido, sigue ahí desde antes
			File.WriteAllText(_rutaArchivo, "[]");

			// Act: una nueva escritura exitosa debe ignorar y
			// sobrescribir el .tmp con contenido válido antes de moverlo.
			_repository.Guardar(new RegistroAuditoria
			{
				Usuario = "admin",
				Modulo = "Productos",
				Accion = TipoAccionAuditoria.Creacion,
				Entidad = "Item luego de interrupcion previa",
				FechaHora = DateTime.Now
			});

			// Assert: el archivo final es JSON válido y contiene el registro nuevo
			var todos = _repository.ObtenerTodos();
			Assert.Single(todos);
			Assert.Equal("Item luego de interrupcion previa", todos[0].Entidad);
			Assert.False(File.Exists(_rutaTemporal));
		}

		[Fact]
		public void Guardar_ElArchivoFinalNuncaQuedaEnEstadoVacioOTruncado()
		{
			// Arrange: un primer registro válido ya guardado
			_repository.Guardar(new RegistroAuditoria
			{
				Usuario = "admin",
				Modulo = "Finanzas",
				Accion = TipoAccionAuditoria.Creacion,
				Entidad = "Movimiento #1",
				FechaHora = DateTime.Now
			});

			var contenidoAntes = File.ReadAllText(_rutaArchivo);

			// Act: agrego un segundo registro
			_repository.Guardar(new RegistroAuditoria
			{
				Usuario = "admin",
				Modulo = "Finanzas",
				Accion = TipoAccionAuditoria.Creacion,
				Entidad = "Movimiento #2",
				FechaHora = DateTime.Now
			});

			// Assert: en ningún momento el archivo queda vacío o con datos
			// parciales — siempre es JSON válido completo
			var contenidoDespues = File.ReadAllText(_rutaArchivo);
			Assert.NotEqual(contenidoAntes, contenidoDespues);

			var todos = _repository.ObtenerTodos();
			Assert.Equal(2, todos.Count);
		}
	}
}