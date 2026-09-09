namespace PizzaPlanetaBiblioteca.DTOs.Pizzas;

public class CrearPizzaDto
{
    public string Nombre { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public decimal Precio { get; set; }

    public string? ImagenUrl { get; set; }
}