using PizzaPlanetaBiblioteca.DTOs;

namespace PizzaPlanetaBiblioteca.IServices;

public interface IAuthService
{
    Task<bool> RegistroAsync(RegistroDto dto, CancellationToken cancellationToken = default);
    Task<AuthRespuestaDto?> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default);
}
