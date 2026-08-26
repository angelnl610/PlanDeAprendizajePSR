using Microsoft.EntityFrameworkCore;
using PizzaPlanetaApi.Datos;

namespace PizzaPlanetaApi.Endpoints;

public static class PedidoEndpoints
{
    public static void MapPedidoEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/pedidos")
            .WithTags("Pedidos");

        // GET Todos
        group.MapGet("/", ObtenerPedidos);

        // GET por Id
        group.MapGet("/{id:int}", ObtenerPedido);

        // POST
        group.MapPost("/", CrearPedido);

        // PUT Estado
        group.MapPut("/{id:int}/estado", ActualizarEstado);

        // DELETE
        group.MapDelete("/{id:int}", EliminarPedido);
    }

    private static async Task<IResult> ObtenerPedidos(PizzeriaDbContext db)
    {
        var pedidos = await db.Pedidos

            .Include(p => p.Cliente)

            .Include(p => p.ItemsPedido)
                .ThenInclude(i => i.Pizza)

            .Select(p => new PedidoDto
            {
                Id = p.Id,

                Fecha = p.Fecha,

                Estado = p.Estado,

                Total = p.Total,

                Cliente = p.Cliente.Nombre,

                Items = p.ItemsPedido.Select(i => new ItemPedidoDto
                {
                    PizzaId = i.PizzaId,

                    Pizza = i.Pizza.Nombre,

                    Cantidad = i.Cantidad,

                    PrecioUnitario = i.PrecioUnitario

                }).ToList()

            })

            .ToListAsync();

        return Results.Ok(pedidos);
    }

    private static async Task<IResult> ObtenerPedido(int id, PizzeriaDbContext db)
    {
        var pedido = await db.Pedidos

            .Include(p => p.Cliente)

            .Include(p => p.ItemsPedido)
                .ThenInclude(i => i.Pizza)

            .Where(p => p.Id == id)

            .Select(p => new PedidoDto
            {
                Id = p.Id,

                Fecha = p.Fecha,

                Estado = p.Estado,

                Total = p.Total,

                Cliente = p.Cliente.Nombre,

                Items = p.ItemsPedido.Select(i => new ItemPedidoDto
                {
                    PizzaId = i.PizzaId,

                    Pizza = i.Pizza.Nombre,

                    Cantidad = i.Cantidad,

                    PrecioUnitario = i.PrecioUnitario

                }).ToList()

            })

            .FirstOrDefaultAsync();

        if (pedido is null)
            return Results.NotFound();

        return Results.Ok(pedido);
    }


    private static async Task<IResult> CrearPedido(
        CrearPedidoDto dto,
        PizzeriaDbContext db)
    {
        // Verificar que exista el cliente
        var cliente = await db.Clientes.FindAsync(dto.ClienteId);

        if (cliente is null)
        {
            return Results.BadRequest("El cliente no existe.");
        }

        // El pedido debe tener al menos un item
        if (dto.Items.Count == 0)
        {
            return Results.BadRequest("Debe agregar al menos una pizza.");
        }

        var pedido = new Pedido
        {
            ClienteId = dto.ClienteId,
            Fecha = DateTime.Now,
            Estado = EstadoPedido.Pendiente
        };

        decimal total = 0;

        foreach (var itemDto in dto.Items)
        {
            var pizza = await db.Pizzas.FindAsync(itemDto.PizzaId);

            if (pizza is null)
            {
                return Results.BadRequest($"La pizza con ID {itemDto.PizzaId} no existe.");
            }

            var item = new ItemPedido
            {
                PizzaId = pizza.Id,
                Cantidad = itemDto.Cantidad,
                PrecioUnitario = pizza.Precio
            };

            pedido.ItemsPedido.Add(item);

            total += pizza.Precio * itemDto.Cantidad;
        }

        pedido.Total = total;

        db.Pedidos.Add(pedido);

        await db.SaveChangesAsync();

        return Results.Created($"/pedidos/{pedido.Id}", new
        {
            pedido.Id,
            pedido.Total,
            pedido.Estado
        });
    }

    private static async Task<IResult> ActualizarEstado(
        int id,
        ActualizarEstadoPedidoDto dto,
        PizzeriaDbContext db)
    {
        var pedido = await db.Pedidos.FindAsync(id);

        if (pedido is null)
        {
            return Results.NotFound();
        }

        pedido.Estado = dto.Estado;

        await db.SaveChangesAsync();

        return Results.NoContent();
    }


    private static async Task<IResult> EliminarPedido(
        int id,
        PizzeriaDbContext db)
    {
        var pedido = await db.Pedidos.FindAsync(id);

        if (pedido is null)
        {
            return Results.NotFound();
        }

        db.Pedidos.Remove(pedido);

        await db.SaveChangesAsync();

        return Results.NoContent();
    }





}