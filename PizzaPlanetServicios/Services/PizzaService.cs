using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using PizzaPlanetaBiblioteca.DTOs;
using PizzaPlanetaBiblioteca.IServices;

namespace PizzaPlanetServicios.Services;

public class PizzaService : IPizzaService
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PizzaService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
    }

    private void AdjuntarBearerToken()
    {
        var token = _httpContextAccessor.HttpContext?.User.FindFirst("TokenJwt")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    public async Task<IEnumerable<PizzaDetalleDto>> ObtenerTodasAsync(CancellationToken cancellationToken = default)
    {
        AdjuntarBearerToken();
        var pizzas = await _httpClient.GetFromJsonAsync<IEnumerable<PizzaDetalleDto>>("/pizzas", cancellationToken);
        return pizzas ?? Enumerable.Empty<PizzaDetalleDto>();
    }

    public async Task<PizzaDetalleDto?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        AdjuntarBearerToken();
        return await _httpClient.GetFromJsonAsync<PizzaDetalleDto>($"/pizzas/{id}", cancellationToken);
    }

    public async Task<PizzaDetalleDto?> CrearAsync(PizzaAltaDto dto, CancellationToken cancellationToken = default)
    {
        AdjuntarBearerToken();
        var response = await _httpClient.PostAsJsonAsync("/pizzas", dto, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<PizzaDetalleDto>(cancellationToken);
    }

    public async Task<bool> ActualizarAsync(int id, PizzaActualizarDto dto, CancellationToken cancellationToken = default)
    {
        AdjuntarBearerToken();
        var response = await _httpClient.PutAsJsonAsync($"/pizzas/{id}", dto, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> EliminarAsync(int id, CancellationToken cancellationToken = default)
    {
        AdjuntarBearerToken();
        var response = await _httpClient.DeleteAsync($"/pizzas/{id}", cancellationToken);
        return response.IsSuccessStatusCode;
    }
}
