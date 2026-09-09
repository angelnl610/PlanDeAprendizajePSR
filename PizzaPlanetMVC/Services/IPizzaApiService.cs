using PizzaPlanetaBiblioteca.DTOs.Pizzas;

namespace PizzaPlanetMVC.Services;

public interface IPizzaApiService
{
    Task<IEnumerable<PizzaDto>> ObtenerPizzasAsync(CancellationToken cancellationToken = default);
    Task<PizzaDto?> ObtenerPizzaPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> CrearPizzaAsync(CrearPizzaDto dto, CancellationToken cancellationToken = default);
    Task<bool> ActualizarPizzaAsync(int id, ActualizarPizzaDto dto, CancellationToken cancellationToken = default);
    Task<bool> EliminarPizzaAsync(int id, CancellationToken cancellationToken = default);
}
