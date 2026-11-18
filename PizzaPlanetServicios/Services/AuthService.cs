using System.Net.Http.Json;
using PizzaPlanetaBiblioteca.DTOs;
using PizzaPlanetaBiblioteca.IServices;

namespace PizzaPlanetServicios.Services;

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;

    public AuthService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> RegistroAsync(RegistroDto dto, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("/auth/registro", dto, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task<AuthRespuestaDto?> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("/auth/login", dto, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<AuthRespuestaDto>(cancellationToken);
    }
}
