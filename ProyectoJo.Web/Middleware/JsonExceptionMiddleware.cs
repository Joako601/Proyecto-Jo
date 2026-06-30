using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ProyectoJo.Web.Middleware
{
	
	public class JsonExceptionMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly ILogger<JsonExceptionMiddleware> _logger;

		public JsonExceptionMiddleware(RequestDelegate next, ILogger<JsonExceptionMiddleware> logger)
		{
			_next = next;
			_logger = logger;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			try
			{
				await _next(context);
			}
			catch (Exception ex)
			{
				if (EsRequestJson(context))
				{
					_logger.LogError(ex,
						"Error no controlado en endpoint JSON {Method} {Path}",
						context.Request.Method,
						context.Request.Path);

					await EscribirRespuestaJsonAsync(context, ex);
				}
				else
				{
					throw; 
				}
			}
		}

		private static bool EsRequestJson(HttpContext context)
		{

			var path = context.Request.Path.Value ?? string.Empty;
			var accept = context.Request.Headers.Accept.ToString();
			var requestedWith = context.Request.Headers["X-Requested-With"].ToString();

			return requestedWith.Equals("XMLHttpRequest", StringComparison.OrdinalIgnoreCase)
				|| accept.Contains("application/json", StringComparison.OrdinalIgnoreCase)
				|| path.Contains("/Operaciones/Cocina/", StringComparison.OrdinalIgnoreCase)
				|| path.Contains("/Operaciones/Recepcion/", StringComparison.OrdinalIgnoreCase);
		}

		private static async Task EscribirRespuestaJsonAsync(HttpContext context, Exception ex)
		{
			context.Response.StatusCode = StatusCodes.Status500InternalServerError;
			context.Response.ContentType = "application/json";

			var problema = new ProblemDetails
			{
				Status = StatusCodes.Status500InternalServerError,
				Title = "Error interno del servidor",
				Detail = "Ocurrió un error inesperado. Por favor intentá de nuevo.",
				Instance = context.Request.Path
			};

			var json = JsonSerializer.Serialize(problema, new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			});

			await context.Response.WriteAsync(json);
		}
	}
}