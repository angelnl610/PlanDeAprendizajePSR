namespace PizzaPlanetMVC.Models;

public class PizzaCatalogoViewModel
{
    public List<PizzaItemViewModel> Pizzas { get; set; } = new();

    public string? SearchTerm { get; set; }

    public string? SortOrder { get; set; } // "asc" (precio menor a mayor), "desc" (precio mayor a menor)

    public bool IsLoading { get; set; }

    public bool HasError { get; set; }

    public string? ErrorMessage { get; set; }

    public int TotalResultados => Pizzas.Count;
}
