using System.Security.Claims; 
using Microsoft.AspNetCore.Authentication; 
using Microsoft.AspNetCore.Authentication.Cookies;
using ProyectoJo.Application.Ports.In;
using ProyectoJo.Application.UseCases;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Infrastructure.Persistence;
using ProyectoJo.Infrastructure.Auth;
using System.Globalization;
using Microsoft.Extensions.FileProviders;


var builder = WebApplication.CreateBuilder(args);

var adminWebRoot = Path.Combine(builder.Environment.ContentRootPath, "Areas", "Admin", "wwwroot");
builder.Environment.WebRootFileProvider = new CompositeFileProvider(
	builder.Environment.WebRootFileProvider,
	new PhysicalFileProvider(adminWebRoot)
);


var rutaPersistencia = Path.Combine(builder.Environment.ContentRootPath, "Persistencia");
var rutaMenu = Path.Combine(rutaPersistencia, "menu.json");
var rutaFinanzas = Path.Combine(rutaPersistencia, "finanzas.json");
var rutaPromociones = Path.Combine(rutaPersistencia, "promociones.json");

builder.Services.AddScoped<IProductoRepository>(_ => new JsonProductRepository(rutaMenu));
builder.Services.AddScoped<IFinanzaRepository>(_ => new JsonFinanzaRepository(rutaFinanzas));

builder.Services.AddScoped<IProductoService, ProductoUseCase>();


builder.Services.AddScoped<IFinanzaService, FinanzaUseCase>();

builder.Services.AddScoped<IAuthService, EnvAuthService>();

builder.Services.AddScoped<IPromocionRepository>(_ => new JsonPromocionRepository(rutaPromociones));
builder.Services.AddScoped<IPromocionService, PromocionUseCase>();

// --- 1. CONFIGURACIÓN DE SERVICIOS (ANTES DE BUILD) ---
builder.Services.AddControllersWithViews();

// Configuración de Autenticación
builder.Services.AddAuthentication("JoCookieAuth")
	.AddCookie("JoCookieAuth", options => {
		options.LoginPath = "/Admin/Login";
		options.AccessDeniedPath = "/Admin/AccesoDenegado";
	});

// Construimos la aplicación
var app = builder.Build();

// --- 2. PIPELINE DE SOLICITUDES (MIDDLEWARE) ---
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // O app.MapStaticAssets() si usas .NET 9
app.UseRouting();

// El orden aquí es vital: Primero Autenticar, luego Autorizar
app.UseAuthentication();
app.UseAuthorization();

// --- 3. RUTAS ---
// Ruta específica para Áreas (debe ir antes de la ruta default)
app.MapControllerRoute(
	name: "areas",
	pattern: "{area:exists}/{controller=Gestion}/{action=Index}/{id?}");

// Ruta por defecto
app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();