using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PizzaPlanetaApi.Modelos.Entidades;

namespace PizzaPlanetaApi.Datos.Config;

public class PedidoConfig : IEntityTypeConfiguration<Pedido>
{
    public void Configure(EntityTypeBuilder<Pedido> builder)
    {
        builder.ToTable("Pedidos");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Fecha)
            .IsRequired();

        builder.Property(p => p.Total)
            .HasPrecision(10, 2);

        builder.Property(p => p.Estado)
            .HasConversion<string>();

        builder.HasMany(p => p.ItemsPedido)
            .WithOne(i => i.Pedido)
            .HasForeignKey(i => i.PedidoId);
    }
}