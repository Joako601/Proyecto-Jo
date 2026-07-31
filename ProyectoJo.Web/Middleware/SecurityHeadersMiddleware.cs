namespace ProyectoJo.Web.Middleware
{
	public class SecurityHeadersMiddleware
	{
		private const string Csp =
			"default-src 'self'; " +
			"script-src 'self' https://cdn.jsdelivr.net; " +
			"style-src 'self' 'unsafe-inline' https://fonts.googleapis.com https://cdn.jsdelivr.net; " +
			"font-src 'self' https://fonts.gstatic.com https://cdn.jsdelivr.net; " +
			"img-src 'self' data:; " +
			"connect-src 'self'; " +
			"object-src 'none'; " +
			"base-uri 'self'; " +
			"frame-ancestors 'none'";

		private readonly RequestDelegate _next;

		public SecurityHeadersMiddleware(RequestDelegate next)
		{
			_next = next;
		}

		public Task InvokeAsync(HttpContext context)
		{
			context.Response.Headers["X-Content-Type-Options"] = "nosniff";
			context.Response.Headers["X-Frame-Options"] = "DENY";
			context.Response.Headers["Content-Security-Policy"] = Csp;

			return _next(context);
		}
	}
}
