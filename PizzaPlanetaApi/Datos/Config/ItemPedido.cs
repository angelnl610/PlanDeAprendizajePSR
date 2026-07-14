using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PizzaPlanetaApi.Modelos.Entidades;

namespace PizzaPlanetaApi.Datos.Config;

public class ItemPedidoConfig : IEntityTypeConfiguration<ItemPedido>
{
    public void Configure(EntityTypeBuilder<ItemPedido> builder)
    {
        builder.ToTable("ItemsPedido");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Cantidad)
            .IsRequired();

        builder.Property(i => i.PrecioUnitario)
            .HasPrecision(10, 2);
    }
}