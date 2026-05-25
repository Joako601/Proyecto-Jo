using Proyecto_Jo_.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. Agregamos el soporte para MVC (Controladores y Vistas)
builder.Services.AddControllersWithViews();

// 2. Inyectamos nuestro servicio JSON de los platillos
builder.Services.AddSingleton<JsonProductService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

// 3. Manejo de archivos estáticos (CSS, JS) estilo .NET 9
app.MapStaticAssets();

// 4. Ruteo de los controladores
app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}")
	.WithStaticAssets();

app.Run();