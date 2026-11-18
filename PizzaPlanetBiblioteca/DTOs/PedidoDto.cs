using System.ComponentModel.DataAnnotations;
using PizzaPlanetaBiblioteca.Enums;

namespace PizzaPlanetaBiblioteca.DTOs;

public class PedidoDetalleDto
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public EstadoPedido Estado { get; set; }
    public decimal Total { get; set; }
    public int UsuarioId { get; set; }
    public string UsuarioNombre { get; set; } = string.Empty;
    public List<ItemPedidoDetalleDto> Items { get; set; } = [];
}

public class ItemPedidoDetalleDto
{
    public int PizzaId { get; set; }
    public string PizzaNombre { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal => Cantidad * PrecioUnitario;
}

public class PedidoAltaDto
{
    public List<ItemPedidoAltaDto> Items { get; set; } = [];
}

public class ItemPedidoAltaDto
{
    [Range(1, int.MaxValue, ErrorMessage = "PizzaId debe ser un identificador válido.")]
    public int PizzaId { get; set; }

    [Range(1, 50, ErrorMessage = "La cantidad debe estar entre 1 y 50.")]
    public int Cantidad { get; set; }
}

public class PedidoEstadoActualizarDto
{
    public EstadoPedido Estado { get; set; }
}
