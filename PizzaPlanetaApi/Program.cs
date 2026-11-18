using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PizzaPlanetaApi.Datos;
using PizzaPlanetaApi.Endpoints;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Configuración de Base de Datos (MySQL con fallback a SQLite)
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

// Configuración de Autenticación JWT Bearer
var jwtKey = builder.Configuration["Jwt:Key"] ?? "ClaveSecretaSuperSeguraPizzaPlaneta2026!Min32Bytes";
var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "PizzaPlanetaApi",
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "PizzaPlanetMVC",
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// OpenAPI & Scalar
builder.Services.AddOpenApi();

var app = builder.Build();

// Inicializar y sembrar datos de prueba
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

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Pizza Planeta API - Documentación Interactiva")
               .WithTheme(ScalarTheme.Moon);
    });
    app.MapGet("/scalar", () => Results.Redirect("/scalar/v1"));
}

app.UseAuthentication();
app.UseAuthorization();

// Mapeo de Endpoints Modulares
app.MapAuthEndpoints();
app.MapPizzaEndpoints();
app.MapPedidoEndpoints();
app.MapUsuarioEndpoints();

app.Run();
