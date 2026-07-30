using ProyectoJo.Application.Ports.In;
using ProyectoJo.Application.UseCases;

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