using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.RateLimiting;
using ProyectoJo.Application.Ports.In;
using ProyectoJo.Application.UseCases;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Infrastructure.Persistence;
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

var rutaPersistencia = Path.Combine(builder.Environment.ContentRootPath, "Persistencia");
var rutaMenu = Path.Combine(rutaPersistencia, "menu.json");
var rutaFinanzas = Path.Combine(rutaPersistencia, "finanzas.json");
var rutaPromociones = Path.Combine(rutaPersistencia, "promociones.json");
var rutaEmpleados = Path.Combine(rutaPersistencia, "empleados.json");
var rutaDispositivos = Path.Combine(rutaPersistencia, "dispositivos.json");
var rutaPedidos = Path.Combine(rutaPersistencia, "pedidos.json");
var rutaCierresCaja = Path.Combine(rutaPersistencia, "cierres-caja.json");
var rutaAuditoria = Path.Combine(rutaPersistencia, "auditoria.json");

builder.Services.AddScoped<IProductoRepository>(_ => new JsonProductRepository(rutaMenu));
builder.Services.AddScoped<IFinanzaRepository>(_ => new JsonFinanzaRepository(rutaFinanzas));
builder.Services.AddScoped<IProductoService, ProductoUseCase>();
builder.Services.AddScoped<IFinanzaService, FinanzaUseCase>();
builder.Services.AddScoped<IAuthService, EnvAuthService>();

builder.Services.AddScoped<IPromocionRepository>(_ => new JsonPromocionRepository(rutaPromociones));
builder.Services.AddScoped<IPromocionService, PromocionUseCase>();

builder.Services.AddScoped<IEmpleadoRepository>(_ => new JsonEmpleadoRepository(rutaEmpleados));
builder.Services.AddScoped<IEmpleadoAuthService, EmpleadoAuthUseCase>();

builder.Services.AddScoped<IDispositivoRepository>(_ => new JsonDispositivoRepository(rutaDispositivos));
builder.Services.AddScoped<IDispositivoService, DispositivoUseCase>();

builder.Services.AddScoped<IPedidoRepository>(_ => new JsonPedidoRepository(rutaPedidos));
builder.Services.AddScoped<IPedidoService, PedidoUseCase>();

builder.Services.AddScoped<ICierreCajaRepository>(_ => new JsonCierreCajaRepository(rutaCierresCaja));
builder.Services.AddScoped<ICierreCajaService, CierreCajaUseCase>();

builder.Services.AddScoped<IAuditoriaRepository>(_ => new JsonAuditoriaRepository(rutaAuditoria));
builder.Services.AddScoped<IAuditoriaService, AuditoriaUseCase>();

builder.Services.AddScoped<IPedidoNotificador, SignalRPedidoNotificador>();


builder.Services.AddSignalR();


builder.Services.AddRateLimiter(options =>
{
	options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

	options.OnRejected = (context, cancellationToken) =>
	{
		var path = context.HttpContext.Request.Path.Value ?? "";


		var destino = path.Contains("/Operaciones/Auth/Login", StringComparison.OrdinalIgnoreCase)
			? "/Operaciones/Auth/Login?bloqueado=true"
			: "/Admin/Login?bloqueado=true";


		context.HttpContext.Response.StatusCode = StatusCodes.Status302Found;
		context.HttpContext.Response.Headers.Location = destino;

		return ValueTask.CompletedTask;
	};

	options.AddPolicy("login-pin", httpContext =>
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

builder.Services.AddAuthentication("JoCookieAuth")
	.AddCookie("JoCookieAuth", options => {
		options.LoginPath = "/Admin/Login";
		options.AccessDeniedPath = "/Admin/AccesoDenegado";
	})
	.AddCookie("OperacionesCookieAuth", options => {
		options.LoginPath = "/Operaciones/Auth/Login";
		options.AccessDeniedPath = "/Operaciones/Auth/Login";
		options.Cookie.Name = "Jo.Operaciones";
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

app.UseSerilogRequestLogging();

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