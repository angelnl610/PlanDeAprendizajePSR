namespace PizzaPlanetaApi.Modelos.DTOs.Pizzas;

public class CrearPizzaDto
{
    public string Nombre { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public decimal Precio { get; set; }
}