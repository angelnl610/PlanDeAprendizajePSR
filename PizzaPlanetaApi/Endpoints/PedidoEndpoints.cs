using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using PizzaPlanetaApi.Datos;
using PizzaPlanetaBiblioteca.DTOs;
using PizzaPlanetaBiblioteca.Entidades;
using PizzaPlanetaBiblioteca.Enums;

namespace PizzaPlanetaApi.Endpoints;

public static class PedidoEndpoints
{
    public static void MapPedidoEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/pedidos")
            .WithTags("Pedidos")
            .RequireAuthorization();

        group.MapGet("/", ObtenerPedidos);
        group.MapGet("/{id:int}", ObtenerPedido);
        group.MapPost("/", CrearPedido);
        group.MapPut("/{id:int}/estado", ActualizarEstado);
        group.MapDelete("/{id:int}", EliminarPedido);
    }

    private static async Task<IResult> ObtenerPedidos(ClaimsPrincipal user, PizzeriaDbContext db)
    {
        var rol = user.FindFirstValue(ClaimTypes.Role);
        var userIdStr = user.FindFirstValue(ClaimTypes.NameIdentifier);
        int.TryParse(userIdStr, out var userId);

        var query = db.Pedidos
            .Include(p => p.Usuario)
            .Include(p => p.ItemsPedido)
                .ThenInclude(i => i.Pizza)
            .AsQueryable();

        // Si es cliente, solo ve sus pedidos
        if (rol == "Cliente")
        {
            query = query.Where(p => p.UsuarioId == userId);
        }

        var pedidos = await query
            .OrderByDescending(p => p.Fecha)
            .Select(p => new PedidoDetalleDto
            {
                Id = p.Id,
                Fecha = p.Fecha,
                Estado = p.Estado,
                Total = p.Total,
                UsuarioId = p.UsuarioId,
                UsuarioNombre = p.Usuario.Nombre,
                Items = p.ItemsPedido.Select(i => new ItemPedidoDetalleDto
                {
                    PizzaId = i.PizzaId,
                    PizzaNombre = i.Pizza != null ? i.Pizza.Nombre : string.Empty,
                    Cantidad = i.Cantidad,
                    PrecioUnitario = i.PrecioUnitario
                }).ToList()
            })
            .ToListAsync();

        return Results.Ok(pedidos);
    }

    private static async Task<IResult> ObtenerPedido(int id, ClaimsPrincipal user, PizzeriaDbContext db)
    {
        var rol = user.FindFirstValue(ClaimTypes.Role);
        var userIdStr = user.FindFirstValue(ClaimTypes.NameIdentifier);
        int.TryParse(userIdStr, out var userId);

        var pedido = await db.Pedidos
            .Include(p => p.Usuario)
            .Include(p => p.ItemsPedido)
                .ThenInclude(i => i.Pizza)
            .Where(p => p.Id == id)
            .Select(p => new PedidoDetalleDto
            {
                Id = p.Id,
                Fecha = p.Fecha,
                Estado = p.Estado,
                Total = p.Total,
                UsuarioId = p.UsuarioId,
                UsuarioNombre = p.Usuario.Nombre,
                Items = p.ItemsPedido.Select(i => new ItemPedidoDetalleDto
                {
                    PizzaId = i.PizzaId,
                    PizzaNombre = i.Pizza != null ? i.Pizza.Nombre : string.Empty,
                    Cantidad = i.Cantidad,
                    PrecioUnitario = i.PrecioUnitario
                }).ToList()
            })
            .FirstOrDefaultAsync();

        if (pedido is null)
            return Results.NotFound();

        if (rol == "Cliente" && pedido.UsuarioId != userId)
            return Results.Forbid();

        return Results.Ok(pedido);
    }

    private static async Task<IResult> CrearPedido(
        PedidoAltaDto dto,
        ClaimsPrincipal user,
        PizzeriaDbContext db,
        IServiceScopeFactory scopeFactory)
    {
        var userIdStr = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdStr, out var usuarioId))
        {
            return Results.Unauthorized();
        }

        if (dto.Items == null || dto.Items.Count == 0 || dto.Items.Any(i => i.Cantidad <= 0))
        {
            return Results.BadRequest(new { error = "ERR_EMPTY_ORDER", mensaje = "Debe agregar al menos una pizza con cantidad positiva." });
        }

        var pizzaIds = dto.Items.Select(i => i.PizzaId).Distinct().ToList();
        var pizzas = await db.Pizzas
            .Where(p => pizzaIds.Contains(p.Id) && p.Activa)
            .ToDictionaryAsync(p => p.Id);

        if (pizzas.Count != pizzaIds.Count)
        {
            return Results.BadRequest(new { error = "ERR_PIZZA_NOT_AVAILABLE", mensaje = "Una o más pizzas no están disponibles." });
        }

        var pedido = new Pedido
        {
            UsuarioId = usuarioId,
            Fecha = DateTime.Now,
            Estado = EstadoPedido.Pendiente
        };

        decimal total = 0;
        foreach (var itemDto in dto.Items)
        {
            var pizza = pizzas[itemDto.PizzaId];
            pedido.ItemsPedido.Add(new ItemPedido
            {
                PizzaId = pizza.Id,
                Cantidad = itemDto.Cantidad,
                PrecioUnitario = pizza.Precio
            });
            total += pizza.Precio * itemDto.Cantidad;
        }
        pedido.Total = total;

        db.Pedidos.Add(pedido);
        await db.SaveChangesAsync();

        var pedidoId = pedido.Id;

        // Simulación asíncrona temporalizada en segundo plano con IServiceScopeFactory seguro
        _ = Task.Run(async () =>
        {
            try
            {
                // 1. Espera 5 segundos -> EnPreparacion
                await Task.Delay(5000);
                using (var scope1 = scopeFactory.CreateScope())
                {
                    var scopedDb = scope1.ServiceProvider.GetRequiredService<PizzeriaDbContext>();
                    var p = await scopedDb.Pedidos.FindAsync(pedidoId);
                    if (p != null && p.Estado == EstadoPedido.Pendiente)
                    {
                        p.Estado = EstadoPedido.EnPreparacion;
                        await scopedDb.SaveChangesAsync();
                    }
                }

                // 2. Espera 5 segundos -> EnCamino
                await Task.Delay(5000);
                using (var scope2 = scopeFactory.CreateScope())
                {
                    var scopedDb = scope2.ServiceProvider.GetRequiredService<PizzeriaDbContext>();
                    var p = await scopedDb.Pedidos.FindAsync(pedidoId);
                    if (p != null && p.Estado == EstadoPedido.EnPreparacion)
                    {
                        p.Estado = EstadoPedido.EnCamino;
                        await scopedDb.SaveChangesAsync();
                    }
                }

                // 3. Espera 5 segundos -> Entregado
                await Task.Delay(5000);
                using (var scope3 = scopeFactory.CreateScope())
                {
                    var scopedDb = scope3.ServiceProvider.GetRequiredService<PizzeriaDbContext>();
                    var p = await scopedDb.Pedidos.FindAsync(pedidoId);
                    if (p != null && p.Estado == EstadoPedido.EnCamino)
                    {
                        p.Estado = EstadoPedido.Entregado;
                        await scopedDb.SaveChangesAsync();
                    }
                }
            }
            catch
            {
                // No bloquear servidor ante errores de background
            }
        });

        return Results.Created($"/pedidos/{pedido.Id}", new PedidoDetalleDto
        {
            Id = pedido.Id,
            Fecha = pedido.Fecha,
            Estado = pedido.Estado,
            Total = pedido.Total,
            UsuarioId = pedido.UsuarioId
        });
    }

    private static async Task<IResult> ActualizarEstado(
        int id,
        PedidoEstadoActualizarDto dto,
        PizzeriaDbContext db)
    {
        var pedido = await db.Pedidos.FindAsync(id);
        if (pedido is null)
            return Results.NotFound();

        pedido.Estado = dto.Estado;
        await db.SaveChangesAsync();

        return Results.NoContent();
    }

    private static async Task<IResult> EliminarPedido(int id, PizzeriaDbContext db)
    {
        var pedido = await db.Pedidos.FindAsync(id);
        if (pedido is null)
            return Results.NotFound();

        db.Pedidos.Remove(pedido);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }
}
