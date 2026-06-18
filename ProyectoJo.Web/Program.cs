using System.Security.Claims; // ¡Esencial para los Claims!
using Microsoft.AspNetCore.Authentication; // ¡Esencial para el SignInAsync!
using Microsoft.AspNetCore.Authentication.Cookies;
using ProyectoJo.Application.Ports.In;
using ProyectoJo.Application.UseCases;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Infrastructure.Persistence;
using ProyectoJo.Infrastructure.Auth;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IProductoRepository, JsonProductRepository>();
builder.Services.AddScoped<IProductoService, ProductoUseCase>();

builder.Services.AddScoped<IAuthService, EnvAuthService>();

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