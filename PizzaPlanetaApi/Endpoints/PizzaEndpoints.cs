using Microsoft.EntityFrameworkCore;
using PizzaPlanetaApi.Datos;
using PizzaPlanetaBiblioteca.DTOs.Pizzas;
using PizzaPlanetaBiblioteca.Entidades;

namespace PizzaPlanetaApi.Endpoints;

public static class PizzaEndpoints
{
    public static void MapPizzaEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/pizzas")
            .WithTags("Pizzas");

        // Obtener todas
        group.MapGet("/", async (PizzeriaDbContext db) =>
        {
            var pizzas = await db.Pizzas
                .Select(p => new PizzaDto
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    Precio = p.Precio,
                    ImagenUrl = p.ImagenUrl
                })
                .ToListAsync();

            return Results.Ok(pizzas);
        });

        // Obtener por Id
        group.MapGet("/{id:int}", async (int id, PizzeriaDbContext db) =>
        {
            var pizza = await db.Pizzas
                .Where(p => p.Id == id)
                .Select(p => new PizzaDto
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    Precio = p.Precio,
                    ImagenUrl = p.ImagenUrl
                })
                .FirstOrDefaultAsync();

            return pizza is null
                ? Results.NotFound()
                : Results.Ok(pizza);
        });

        // Crear
        group.MapPost("/", async (CrearPizzaDto dto, PizzeriaDbContext db) =>
        {
            var pizza = new Pizza
            {
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                Precio = dto.Precio,
                ImagenUrl = dto.ImagenUrl
            };

            db.Pizzas.Add(pizza);

            await db.SaveChangesAsync();

            var pizzaDto = new PizzaDto
            {
                Id = pizza.Id,
                Nombre = pizza.Nombre,
                Descripcion = pizza.Descripcion,
                Precio = pizza.Precio,
                ImagenUrl = pizza.ImagenUrl
            };

            return Results.Created($"/pizzas/{pizza.Id}", pizzaDto);
        });

        // Actualizar
        group.MapPut("/{id:int}", async (int id, ActualizarPizzaDto dto, PizzeriaDbContext db) =>
        {
            var pizza = await db.Pizzas.FindAsync(id);

            if (pizza is null)
                return Results.NotFound();

            pizza.Nombre = dto.Nombre;
            pizza.Descripcion = dto.Descripcion;
            pizza.Precio = dto.Precio;
            pizza.ImagenUrl = dto.ImagenUrl;

            await db.SaveChangesAsync();

            return Results.NoContent();
        });

        // Eliminar
        group.MapDelete("/{id:int}", async (int id, PizzeriaDbContext db) =>
        {
            var pizza = await db.Pizzas.FindAsync(id);

            if (pizza is null)
                return Results.NotFound();

            db.Pizzas.Remove(pizza);

            await db.SaveChangesAsync();

            return Results.NoContent();
        });
    }
}