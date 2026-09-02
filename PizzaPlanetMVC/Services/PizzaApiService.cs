using System.Net.Http.Json;
using PizzaPlanetaBiblioteca.DTOs.Pizzas;

namespace PizzaPlanetMVC.Services;

public class PizzaApiService : IPizzaApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PizzaApiService> _logger;

    public PizzaApiService(HttpClient httpClient, ILogger<PizzaApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IEnumerable<PizzaDto>> ObtenerPizzasAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var pizzas = await _httpClient.GetFromJsonAsync<IEnumerable<PizzaDto>>("/pizzas", cancellationToken);
            return pizzas ?? Enumerable.Empty<PizzaDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar las pizzas de forma asíncrona desde la API.");
            return Enumerable.Empty<PizzaDto>();
        }
    }

    public async Task<PizzaDto?> ObtenerPizzaPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<PizzaDto>($"/pizzas/{id}", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar la pizza con id {Id} de forma asíncrona desde la API.", id);
            return null;
        }
    }
}
