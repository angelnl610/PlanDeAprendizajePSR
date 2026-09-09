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

    public async Task<bool> CrearPizzaAsync(CrearPizzaDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/pizzas", dto, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear la pizza de forma asíncrona mediante la API.");
            return false;
        }
    }

    public async Task<bool> ActualizarPizzaAsync(int id, ActualizarPizzaDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"/pizzas/{id}", dto, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar la pizza con id {Id} de forma asíncrona mediante la API.", id);
            return false;
        }
    }

    public async Task<bool> EliminarPizzaAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/pizzas/{id}", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar la pizza con id {Id} de forma asíncrona mediante la API.", id);
            return false;
        }
    }
}
