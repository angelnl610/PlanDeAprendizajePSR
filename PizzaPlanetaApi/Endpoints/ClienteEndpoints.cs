using Microsoft.EntityFrameworkCore;
using PizzaPlanetaApi.Datos;


namespace PizzaPlanetaApi.Endpoints;

public static class ClienteEndpoints
{
    public static void MapClienteEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/clientes")
            .WithTags("Clientes");

        // Obtener todos
        group.MapGet("/", async (PizzeriaDbContext db) =>
        {
            var clientes = await db.Clientes
                .Select(c => new ClienteDto
                {
                    Id = c.Id,
                    Nombre = c.Nombre,
                    Telefono = c.Telefono,
                    Direccion = c.Direccion
                })
                .ToListAsync();

            return Results.Ok(clientes);
        });

        // Obtener por Id
        group.MapGet("/{id:int}", async (int id, PizzeriaDbContext db) =>
        {
            var cliente = await db.Clientes
                .Where(c => c.Id == id)
                .Select(c => new ClienteDto
                {
                    Id = c.Id,
                    Nombre = c.Nombre,
                    Telefono = c.Telefono,
                    Direccion = c.Direccion
                })
                .FirstOrDefaultAsync();

            return cliente is null
                ? Results.NotFound()
                : Results.Ok(cliente);
        });

        // Crear
        group.MapPost("/", async (CrearClienteDto dto, PizzeriaDbContext db) =>
        {
            var cliente = new Cliente
            {
                Nombre = dto.Nombre,
                Telefono = dto.Telefono,
                Direccion = dto.Direccion
            };

            db.Clientes.Add(cliente);

            await db.SaveChangesAsync();

            var resultado = new ClienteDto
            {
                Id = cliente.Id,
                Nombre = cliente.Nombre,
                Telefono = cliente.Telefono,
                Direccion = cliente.Direccion
            };

            return Results.Created($"/clientes/{cliente.Id}", resultado);
        });

        // Actualizar
        group.MapPut("/{id:int}", async (int id, ActualizarClienteDto dto, PizzeriaDbContext db) =>
        {
            var cliente = await db.Clientes.FindAsync(id);

            if (cliente is null)
                return Results.NotFound();

            cliente.Nombre = dto.Nombre;
            cliente.Telefono = dto.Telefono;
            cliente.Direccion = dto.Direccion;

            await db.SaveChangesAsync();

            return Results.NoContent();
        });

        // Eliminar
        group.MapDelete("/{id:int}", async (int id, PizzeriaDbContext db) =>
        {
            var cliente = await db.Clientes.FindAsync(id);

            if (cliente is null)
                return Results.NotFound();

            db.Clientes.Remove(cliente);

            await db.SaveChangesAsync();

            return Results.NoContent();
        });
    }
}