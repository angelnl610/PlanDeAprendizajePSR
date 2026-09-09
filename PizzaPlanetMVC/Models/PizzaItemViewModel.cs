namespace PizzaPlanetMVC.Models;

public class PizzaItemViewModel
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public decimal Precio { get; set; }

    public string? ImagenUrl { get; set; }

    public string PrecioFormateado => $"${Precio:0.00}";
}
