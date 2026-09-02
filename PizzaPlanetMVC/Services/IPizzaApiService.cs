using PizzaPlanetaBiblioteca.DTOs.Pizzas;

namespace PizzaPlanetMVC.Services;

public interface IPizzaApiService
{
    Task<IEnumerable<PizzaDto>> ObtenerPizzasAsync(CancellationToken cancellationToken = default);
    Task<PizzaDto?> ObtenerPizzaPorIdAsync(int id, CancellationToken cancellationToken = default);
}
