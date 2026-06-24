using ProyectoJo.Application.Ports.In;
using ProyectoJo.Application.UseCases;
using ProyectoJo.Application.Ports.Out;
using ProyectoJo.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
	.AddJsonOptions(options =>
	{
		options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
	});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
	c.SwaggerDoc("v1", new()
	{
		Title = "Proyecto Jo' — Orders API",
		Version = "v1",
		Description = "REST API for order management between front desk and kitchen."
	});
});

var pedidosPath = Path.Combine(
	builder.Environment.ContentRootPath, "..", "ProyectoJo.Web", "Persistencia", "pedidos.json");

var menuPath = Path.Combine(
	builder.Environment.ContentRootPath, "..", "ProyectoJo.Web", "Persistencia", "menu.json");

var finanzasPath = Path.Combine(
	builder.Environment.ContentRootPath, "..", "ProyectoJo.Web", "Persistencia", "finanzas.json");

builder.Services.AddSingleton<IPedidoRepository>(
	new JsonPedidoRepository(Path.GetFullPath(pedidosPath)));

builder.Services.AddSingleton<IProductoRepository>(
	new JsonProductRepository(Path.GetFullPath(menuPath)));

builder.Services.AddSingleton<IFinanzaRepository>(
	new JsonFinanzaRepository(Path.GetFullPath(finanzasPath)));

builder.Services.AddScoped<IPedidoService, PedidoUseCase>();
builder.Services.AddScoped<IProductoService, ProductoUseCase>();
builder.Services.AddScoped<IFinanzaService, FinanzaUseCase>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
	c.SwaggerEndpoint("/swagger/v1/swagger.json", "Proyecto Jo' API v1");
	c.RoutePrefix = "swagger";
});

app.UseAuthorization();
app.MapControllers();
app.Run();