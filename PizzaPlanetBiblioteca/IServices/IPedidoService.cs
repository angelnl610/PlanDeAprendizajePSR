using PizzaPlanetaBiblioteca.DTOs;

namespace PizzaPlanetaBiblioteca.IServices;

public interface IPedidoService
{
    Task<IEnumerable<PedidoDetalleDto>> ObtenerTodosAsync(CancellationToken cancellationToken = default);
    Task<PedidoDetalleDto?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PedidoDetalleDto?> CrearAsync(PedidoAltaDto dto, CancellationToken cancellationToken = default);
    Task<bool> ActualizarEstadoAsync(int id, PedidoEstadoActualizarDto dto, CancellationToken cancellationToken = default);
    Task<bool> EliminarAsync(int id, CancellationToken cancellationToken = default);
}
