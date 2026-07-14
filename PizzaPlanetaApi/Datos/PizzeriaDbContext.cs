using Microsoft.EntityFrameworkCore;
using PizzaPlanetaApi.Modelos.Entidades;

namespace PizzaPlanetaApi.Datos;

public class PizzeriaDbContext : DbContext
{
    public PizzeriaDbContext(DbContextOptions<PizzeriaDbContext> options)
        : base(options)
    {
    }

    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Pizza> Pizzas => Set<Pizza>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<ItemPedido> ItemsPedido => Set<ItemPedido>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PizzeriaDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}