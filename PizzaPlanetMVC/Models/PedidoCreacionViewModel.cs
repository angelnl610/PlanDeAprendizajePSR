namespace PizzaPlanetMVC.Models;

public class PedidoCreacionViewModel
{
    public List<ItemSeleccionPizzaViewModel> Pizzas { get; set; } = [];
}

public class ItemSeleccionPizzaViewModel
{
    public int PizzaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public int Cantidad { get; set; } = 0;
}
