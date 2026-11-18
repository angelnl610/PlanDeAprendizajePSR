using PizzaPlanetaBiblioteca.DTOs;

namespace PizzaPlanetaBiblioteca.IServices;

public interface IUsuarioService
{
    Task<IEnumerable<UsuarioDetalleDto>> ObtenerTodosAsync(CancellationToken cancellationToken = default);
    Task<UsuarioDetalleDto?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> CambiarRolAsync(int id, UsuarioRolActualizarDto dto, CancellationToken cancellationToken = default);
}
