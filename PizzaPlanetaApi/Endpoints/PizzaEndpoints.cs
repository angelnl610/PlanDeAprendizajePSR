using Microsoft.EntityFrameworkCore;
using PizzaPlanetaApi.Datos;
using PizzaPlanetaBiblioteca.DTOs;
using PizzaPlanetaBiblioteca.Entidades;

namespace PizzaPlanetaApi.Endpoints;

public static class PizzaEndpoints
{
    public static void MapPizzaEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/pizzas")
            .WithTags("Pizzas");

        // Obtener todas (solo activas gracias al QueryFilter)
        group.MapGet("/", async (PizzeriaDbContext db) =>
        {
            var pizzas = await db.Pizzas
                .Select(p => new PizzaDetalleDto
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    Precio = p.Precio,
                    ImagenUrl = p.ImagenUrl,
                    Activa = p.Activa
                })
                .ToListAsync();

            return Results.Ok(pizzas);
        }).RequireAuthorization();

        // Obtener por Id
        group.MapGet("/{id:int}", async (int id, PizzeriaDbContext db) =>
        {
            var pizza = await db.Pizzas
                .Where(p => p.Id == id)
                .Select(p => new PizzaDetalleDto
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    Precio = p.Precio,
                    ImagenUrl = p.ImagenUrl,
                    Activa = p.Activa
                })
                .FirstOrDefaultAsync();

            return pizza is null
                ? Results.NotFound()
                : Results.Ok(pizza);
        }).RequireAuthorization();

        // Crear (Admin o Cocinero)
        group.MapPost("/", async (PizzaAltaDto dto, PizzeriaDbContext db) =>
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre) || dto.Precio <= 0)
                return Results.BadRequest(new { error = "ERR_INVALID_PRICE", mensaje = "Parámetros de pizza inválidos." });

            var pizza = new Pizza
            {
                Nombre = dto.Nombre.Trim(),
                Descripcion = dto.Descripcion?.Trim() ?? string.Empty,
                Precio = dto.Precio,
                ImagenUrl = string.IsNullOrWhiteSpace(dto.ImagenUrl) ? null : dto.ImagenUrl.Trim(),
                Activa = true
            };

            db.Pizzas.Add(pizza);
            await db.SaveChangesAsync();

            var pizzaDto = new PizzaDetalleDto
            {
                Id = pizza.Id,
                Nombre = pizza.Nombre,
                Descripcion = pizza.Descripcion,
                Precio = pizza.Precio,
                ImagenUrl = pizza.ImagenUrl,
                Activa = pizza.Activa
            };

            return Results.Created($"/pizzas/{pizza.Id}", pizzaDto);
        }).RequireAuthorization(policy => policy.RequireRole("Admin", "Cocinero"));

        // Actualizar (Admin o Cocinero)
        group.MapPut("/{id:int}", async (int id, PizzaActualizarDto dto, PizzeriaDbContext db) =>
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre) || dto.Precio <= 0)
                return Results.BadRequest(new { error = "ERR_INVALID_PRICE", mensaje = "Parámetros de pizza inválidos." });

            var pizza = await db.Pizzas.FindAsync(id);
            if (pizza is null)
                return Results.NotFound();

            pizza.Nombre = dto.Nombre.Trim();
            pizza.Descripcion = dto.Descripcion?.Trim() ?? string.Empty;
            pizza.Precio = dto.Precio;
            pizza.ImagenUrl = string.IsNullOrWhiteSpace(dto.ImagenUrl) ? null : dto.ImagenUrl.Trim();

            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization(policy => policy.RequireRole("Admin", "Cocinero"));

        // Soft Delete (Admin o Cocinero)
        group.MapDelete("/{id:int}", async (int id, PizzeriaDbContext db) =>
        {
            var pizza = await db.Pizzas.FindAsync(id);
            if (pizza is null)
                return Results.NotFound();

            pizza.Activa = false;
            await db.SaveChangesAsync();

            return Results.NoContent();
        }).RequireAuthorization(policy => policy.RequireRole("Admin", "Cocinero"));
    }
}
