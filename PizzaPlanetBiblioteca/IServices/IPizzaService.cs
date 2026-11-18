using PizzaPlanetaBiblioteca.DTOs;

namespace PizzaPlanetaBiblioteca.IServices;

public interface IPizzaService
{
    Task<IEnumerable<PizzaDetalleDto>> ObtenerTodasAsync(CancellationToken cancellationToken = default);
    Task<PizzaDetalleDto?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PizzaDetalleDto?> CrearAsync(PizzaAltaDto dto, CancellationToken cancellationToken = default);
    Task<bool> ActualizarAsync(int id, PizzaActualizarDto dto, CancellationToken cancellationToken = default);
    Task<bool> EliminarAsync(int id, CancellationToken cancellationToken = default);
}
