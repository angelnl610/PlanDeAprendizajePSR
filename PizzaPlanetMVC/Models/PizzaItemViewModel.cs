namespace PizzaPlanetMVC.Models;

public class PizzaItemViewModel
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public decimal Precio { get; set; }

    public string ImagenUrl { get; set; } = "/images/pizzas/pizza-default.png";

    public string PrecioFormateado => $"${Precio:0.00}";

    public static string ResolverImagen(string nombre)
    {
        var n = nombre.ToLowerInvariant();
        if (n.Contains("pepperoni") || n.Contains("supernova"))
            return "/images/pizzas/pizza-supernova-pepperoni.png";
        if (n.Contains("bbq") || n.Contains("meteorito"))
            return "/images/pizzas/pizza-meteorito-bbq.png";
        if (n.Contains("vegano") || n.Contains("vegetariana"))
            return "/images/pizzas/pizza-planeta-vegano.png";
        if (n.Contains("hula") || n.Contains("anillo") || n.Contains("anana") || n.Contains("piña"))
            return "/images/pizzas/pizza-hula-hula-anillo.png";
        if (n.Contains("queso") || n.Contains("nebula"))
            return "/images/pizzas/pizza-cuatro-quesos-nebula.png";
        if (n.Contains("fuego") || n.Contains("intergalactico") || n.Contains("picante"))
            return "/images/pizzas/pizza-fuego-intergalactico.png";

        return "/images/pizzas/pizza-default.png";
    }
}
