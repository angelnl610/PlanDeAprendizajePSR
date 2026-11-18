using Microsoft.EntityFrameworkCore;
using PizzaPlanetaApi.Datos;
using PizzaPlanetaBiblioteca.DTOs;

namespace PizzaPlanetaApi.Endpoints;

public static class UsuarioEndpoints
{
    public static void MapUsuarioEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/usuarios")
            .WithTags("Usuarios")
            .RequireAuthorization(policy => policy.RequireRole("Admin"));

        // GET /usuarios
        group.MapGet("/", async (PizzeriaDbContext db) =>
        {
            var usuarios = await db.Usuarios
                .OrderBy(u => u.Id)
                .Select(u => new UsuarioDetalleDto
                {
                    Id = u.Id,
                    Nombre = u.Nombre,
                    Email = u.Email,
                    Rol = u.Rol,
                    Telefono = u.Telefono,
                    Direccion = u.Direccion
                })
                .ToListAsync();

            return Results.Ok(usuarios);
        });

        // GET /usuarios/{id}
        group.MapGet("/{id:int}", async (int id, PizzeriaDbContext db) =>
        {
            var usuario = await db.Usuarios
                .Where(u => u.Id == id)
                .Select(u => new UsuarioDetalleDto
                {
                    Id = u.Id,
                    Nombre = u.Nombre,
                    Email = u.Email,
                    Rol = u.Rol,
                    Telefono = u.Telefono,
                    Direccion = u.Direccion
                })
                .FirstOrDefaultAsync();

            return usuario is null ? Results.NotFound() : Results.Ok(usuario);
        });

        // PUT /usuarios/{id}/rol
        group.MapPut("/{id:int}/rol", async (int id, UsuarioRolActualizarDto dto, PizzeriaDbContext db) =>
        {
            var usuario = await db.Usuarios.FindAsync(id);
            if (usuario is null)
                return Results.NotFound();

            usuario.Rol = dto.Rol;
            await db.SaveChangesAsync();

            return Results.NoContent();
        });
    }
}
