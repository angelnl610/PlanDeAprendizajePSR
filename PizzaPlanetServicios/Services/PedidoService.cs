using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using PizzaPlanetaBiblioteca.DTOs;
using PizzaPlanetaBiblioteca.IServices;

namespace PizzaPlanetServicios.Services;

public class PedidoService : IPedidoService
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PedidoService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
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

    public async Task<IEnumerable<PedidoDetalleDto>> ObtenerTodosAsync(CancellationToken cancellationToken = default)
    {
        AdjuntarBearerToken();
        var pedidos = await _httpClient.GetFromJsonAsync<IEnumerable<PedidoDetalleDto>>("/pedidos", cancellationToken);
        return pedidos ?? Enumerable.Empty<PedidoDetalleDto>();
    }

    public async Task<PedidoDetalleDto?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        AdjuntarBearerToken();
        return await _httpClient.GetFromJsonAsync<PedidoDetalleDto>($"/pedidos/{id}", cancellationToken);
    }

    public async Task<PedidoDetalleDto?> CrearAsync(PedidoAltaDto dto, CancellationToken cancellationToken = default)
    {
        AdjuntarBearerToken();
        var response = await _httpClient.PostAsJsonAsync("/pedidos", dto, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<PedidoDetalleDto>(cancellationToken);
    }

    public async Task<bool> ActualizarEstadoAsync(int id, PedidoEstadoActualizarDto dto, CancellationToken cancellationToken = default)
    {
        AdjuntarBearerToken();
        var response = await _httpClient.PutAsJsonAsync($"/pedidos/{id}/estado", dto, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> EliminarAsync(int id, CancellationToken cancellationToken = default)
    {
        AdjuntarBearerToken();
        var response = await _httpClient.DeleteAsync($"/pedidos/{id}", cancellationToken);
        return response.IsSuccessStatusCode;
    }
}
