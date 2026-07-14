using Microsoft.EntityFrameworkCore;
using PizzaPlanetaApi.Datos;
using PizzaPlanetaApi.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Connection String
var connectionString = builder.Configuration.GetConnectionString("PizzeriaDb");

builder.Services.AddDbContext<PizzeriaDbContext>(options =>
{
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)
    );
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Endpoints
app.MapClienteEndpoints();
app.MapPizzaEndpoints();
app.MapPedidoEndpoints();

app.MapGet("/", () =>
{
    return Results.Ok("Pizza Planeta biri biri");
});

app.Run();