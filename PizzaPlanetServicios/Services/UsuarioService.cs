using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using PizzaPlanetaBiblioteca.DTOs;
using PizzaPlanetaBiblioteca.IServices;

namespace PizzaPlanetServicios.Services;

public class UsuarioService : IUsuarioService
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UsuarioService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
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

    public async Task<IEnumerable<UsuarioDetalleDto>> ObtenerTodosAsync(CancellationToken cancellationToken = default)
    {
        AdjuntarBearerToken();
        var usuarios = await _httpClient.GetFromJsonAsync<IEnumerable<UsuarioDetalleDto>>("/usuarios", cancellationToken);
        return usuarios ?? Enumerable.Empty<UsuarioDetalleDto>();
    }

    public async Task<UsuarioDetalleDto?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        AdjuntarBearerToken();
        return await _httpClient.GetFromJsonAsync<UsuarioDetalleDto>($"/usuarios/{id}", cancellationToken);
    }

    public async Task<bool> CambiarRolAsync(int id, UsuarioRolActualizarDto dto, CancellationToken cancellationToken = default)
    {
        AdjuntarBearerToken();
        var response = await _httpClient.PutAsJsonAsync($"/usuarios/{id}/rol", dto, cancellationToken);
        return response.IsSuccessStatusCode;
    }
}
