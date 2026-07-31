using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using ProyectoJo.Application.Ports.In;
using ProyectoJo.Application.UseCases;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Infrastructure.Persistence.EfCore;
using ProyectoJo.Infrastructure.Persistence.EfCore.Repositories;
using ProyectoJo.Infrastructure.Auth;
using Microsoft.Extensions.FileProviders;
using Serilog;
using ProyectoJo.Web.Hubs;
using ProyectoJo.Web.Realtime;
using System.Threading.RateLimiting;

Log.Logger = new LoggerConfiguration()
	.MinimumLevel.Information()
	.MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
	.Enrich.FromLogContext()
	.WriteTo.Console()
	.WriteTo.File(
		path: "Logs/proyectojo-.log",
		rollingInterval: RollingInterval.Day,
		retainedFileCountLimit: 14)
	.CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

var adminWebRoot = Path.Combine(builder.Environment.ContentRootPath, "Areas", "Admin", "wwwroot");
builder.Environment.WebRootFileProvider = new CompositeFileProvider(
	builder.Environment.WebRootFileProvider,
	new PhysicalFileProvider(adminWebRoot)
);

builder.Services.AddDbContext<ProyectoJoDbContext>(options => options
	.UseNpgsql(builder.Configuration.GetConnectionString("Default"))
	.UseSnakeCaseNamingConvention());

builder.Services.AddScoped<IProductoRepository, EfProductoRepository>();
builder.Services.AddScoped<IFinanzaRepository, EfFinanzaRepository>();
builder.Services.AddScoped<IProductoService, ProductoUseCase>();
builder.Services.AddScoped<IFinanzaService, FinanzaUseCase>();
builder.Services.AddScoped<IAuthService, EnvAuthService>();

builder.Services.AddScoped<IPromocionRepository, EfPromocionRepository>();
builder.Services.AddScoped<IPromocionService, PromocionUseCase>();

builder.Services.AddScoped<IEmpleadoRepository, EfEmpleadoRepository>();
builder.Services.AddScoped<IEmpleadoAuthService, EmpleadoAuthUseCase>();

builder.Services.AddScoped<IDispositivoRepository, EfDispositivoRepository>();
builder.Services.AddScoped<IDispositivoService, DispositivoUseCase>();

builder.Services.AddScoped<ISupervisorClaveRepository, EfSupervisorClaveRepository>();
builder.Services.AddScoped<ISupervisorAuthService, SupervisorAuthUseCase>();

builder.Services.AddScoped<IPedidoRepository, EfPedidoRepository>();
builder.Services.AddScoped<IPedidoService, PedidoUseCase>();

builder.Services.AddScoped<ICierreCajaRepository, EfCierreCajaRepository>();
builder.Services.AddScoped<ICierreCajaService, CierreCajaUseCase>();

builder.Services.AddScoped<IAuditoriaRepository, EfAuditoriaRepository>();
builder.Services.AddScoped<IAuditoriaService, AuditoriaUseCase>();

builder.Services.AddScoped<IPedidoNotificador, SignalRPedidoNotificador>();

builder.Services.AddScoped<IRecetaRepository, EfRecetaRepository>();
builder.Services.AddScoped<IRecetaService, RecetaUseCase>();

builder.Services.AddScoped<IOpinionRepository, EfOpinionRepository>();
builder.Services.AddScoped<IOpinionService, OpinionUseCase>();

builder.Services.AddScoped<IInsumoRepository, EfInsumoRepository>();
builder.Services.AddScoped<IInsumoService, InsumoUseCase>();

builder.Services.AddScoped<IReporteService, ReporteUseCase>();

builder.Services.AddScoped<IAdministradorRepository, EfAdministradorRepository>();
builder.Services.AddScoped<IAdministradorService, AdministradorUseCase>();

builder.Services.AddScoped<IEmpleadoService, EmpleadoUseCase>();


builder.Services.AddSignalR();


builder.Services.AddRateLimiter(options =>
{
	options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

	options.OnRejected = (context, cancellationToken) =>
	{
		var path = context.HttpContext.Request.Path.Value ?? "";


		string destino;
		if (path.Contains("/Operaciones/Auth/LoginSupervisor", StringComparison.OrdinalIgnoreCase))
			destino = "/Operaciones/Auth/LoginSupervisor?bloqueado=true";
		else if (path.Contains("/Operaciones/Auth/Login", StringComparison.OrdinalIgnoreCase))
			destino = "/Operaciones/Auth/Login?bloqueado=true";
		else
			destino = "/Admin/Login?bloqueado=true";


		context.HttpContext.Response.StatusCode = StatusCodes.Status302Found;
		context.HttpContext.Response.Headers.Location = destino;

		return ValueTask.CompletedTask;
	};

	options.AddPolicy("login-operador", httpContext =>
		RateLimitPartition.GetFixedWindowLimiter(
			partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "desconocido",
			factory: _ => new FixedWindowRateLimiterOptions
			{
				PermitLimit = 5,
				Window = TimeSpan.FromMinutes(1),
				QueueLimit = 0
			}));

	options.AddPolicy("login-supervisor", httpContext =>
		RateLimitPartition.GetFixedWindowLimiter(
			partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "desconocido",
			factory: _ => new FixedWindowRateLimiterOptions
			{
				PermitLimit = 5,
				Window = TimeSpan.FromMinutes(1),
				QueueLimit = 0
			}));

	options.AddPolicy("login-admin", httpContext =>
		RateLimitPartition.GetFixedWindowLimiter(
			partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "desconocido",
			factory: _ => new FixedWindowRateLimiterOptions
			{
				PermitLimit = 8,
				Window = TimeSpan.FromMinutes(1),
				QueueLimit = 0
			}));
});

builder.Services.AddControllersWithViews(options =>
{
	options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());
})
	.AddJsonOptions(options =>
	{
		options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
	});

// Sin esto, el filtro global de arriba solo valida el token vía campo de
// formulario "__RequestVerificationToken". Los fetch() de cocina.js/recepcion.js
// mandan el token en un header (JSON y form-urlencoded por igual), así que hay
// que decirle al middleware que también lo busque ahí.
builder.Services.AddAntiforgery(options =>
{
	options.HeaderName = "RequestVerificationToken";
});

static void EndurecerCookie(CookieBuilder cookie)
{
	cookie.HttpOnly = true;
	cookie.SecurePolicy = CookieSecurePolicy.Always;
	cookie.SameSite = SameSiteMode.Strict;
}

builder.Services.AddAuthentication("JoCookieAuth")
	.AddCookie("JoCookieAuth", options => {
		options.LoginPath = "/Admin/Login";
		options.AccessDeniedPath = "/Admin/AccesoDenegado";
		options.Cookie.Name = "Jo.Admin";
		EndurecerCookie(options.Cookie);
		options.ExpireTimeSpan = TimeSpan.FromMinutes(45);
		options.SlidingExpiration = true;
	})
	.AddCookie("SupervisorAuth", options => {
		// Esquema propio de Operaciones, completamente separado de "JoCookieAuth".
		// Una clave de supervisor filtrada no da acceso al panel Admin, y viceversa.
		options.LoginPath = "/Operaciones/Auth/LoginSupervisor";
		options.AccessDeniedPath = "/Operaciones/Auth/LoginSupervisor";
		options.Cookie.Name = "Jo.Supervisor";
		EndurecerCookie(options.Cookie);
		options.ExpireTimeSpan = TimeSpan.FromMinutes(15);
		options.SlidingExpiration = false;
	})
	.AddCookie("OperacionesCookieAuth", options => {
		options.LoginPath = "/Operaciones/Auth/Login";
		options.AccessDeniedPath = "/Operaciones/Auth/Login";
		options.Cookie.Name = "Jo.Operaciones";
		EndurecerCookie(options.Cookie);
		options.ExpireTimeSpan = TimeSpan.FromHours(12);
		options.SlidingExpiration = true;
		options.Events.OnRedirectToLogin = context =>
		{
			if (context.Request.Headers.ContainsKey("X-Requested-With") ||
				context.Request.Path.Value?.Contains("/Operaciones/Recepcion/") == true ||
				context.Request.Path.Value?.Contains("/Operaciones/Cocina/") == true)
			{
				context.Response.StatusCode = 401;
				return Task.CompletedTask;
			}
			context.Response.Redirect(context.RedirectUri);
			return Task.CompletedTask;
		};
	});

var app = builder.Build();

if (args.Contains("--reset"))
{
	using var scope = app.Services.CreateScope();
	var dbContext = scope.ServiceProvider.GetRequiredService<ProyectoJoDbContext>();
	await ProyectoJo.Infrastructure.Persistence.EfCore.JsonToPostgresSeeder.ResetAsync(dbContext);
	return;
}

if (args.Contains("--seed"))
{
	using var scope = app.Services.CreateScope();
	var dbContext = scope.ServiceProvider.GetRequiredService<ProyectoJoDbContext>();
	var rutaPersistencia = Path.Combine(builder.Environment.ContentRootPath, "Persistencia");
	await ProyectoJo.Infrastructure.Persistence.EfCore.JsonToPostgresSeeder.SeedAsync(dbContext, rutaPersistencia);
	return;
}

app.UseSerilogRequestLogging();

app.UseMiddleware<ProyectoJo.Web.Middleware.SecurityHeadersMiddleware>();
app.UseMiddleware<ProyectoJo.Web.Middleware.JsonExceptionMiddleware>();

if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	app.UseHsts();
}
else
{
	app.UseExceptionHandler("/Home/Error");
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
	name: "areas",
	pattern: "{area:exists}/{controller=Gestion}/{action=Index}/{id?}");

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");


app.MapHub<PedidosHub>("/hubs/pedidos");

app.Run();
