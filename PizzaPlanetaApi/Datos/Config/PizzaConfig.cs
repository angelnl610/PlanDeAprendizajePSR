using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PizzaPlanetaApi.Datos.Config;

public class PizzaConfig : IEntityTypeConfiguration<Pizza>
{
    public void Configure(EntityTypeBuilder<Pizza> builder)
    {
        builder.ToTable("Pizzas");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Nombre)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Descripcion)
            .HasMaxLength(300);

        builder.Property(p => p.Precio)
            .HasPrecision(10, 2);

        builder.HasMany(p => p.ItemsPedido)
            .WithOne(i => i.Pizza)
            .HasForeignKey(i => i.PizzaId);
    }
}