using Microsoft.EntityFrameworkCore;
using PizzaPlanetaApi.Datos;
using PizzaPlanetaApi.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Connection String
var connectionString = builder.Configuration.GetConnectionString("PizzeriaDb");

builder.Services.AddDbContext<PizzeriaDbContext>(options =>
{
    if (string.IsNullOrWhiteSpace(connectionString) ||
        connectionString.Contains("USUARIO") ||
        connectionString.StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase))
    {
        var sqliteConn = connectionString?.StartsWith("Data Source=") == true
            ? connectionString
            : "Data Source=pizzaplaneta.db";
        options.UseSqlite(sqliteConn);
    }
    else
    {
        try
        {
            options.UseMySql(
                connectionString,
                ServerVersion.AutoDetect(connectionString)
            );
        }
        catch
        {
            options.UseSqlite("Data Source=pizzaplaneta.db");
        }
    }
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Inicializar y sembrar datos de prueba en la base de datos
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PizzeriaDbContext>();
    try
    {
        DbSeeder.SeedData(db);
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "Error al inicializar la base de datos.");
    }
}

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

app.MapPost("/seed", (PizzeriaDbContext db) =>
{
    DbSeeder.SeedData(db);
    return Results.Ok(new { mensaje = "Datos seed generados exitosamente." });
});

app.Run();