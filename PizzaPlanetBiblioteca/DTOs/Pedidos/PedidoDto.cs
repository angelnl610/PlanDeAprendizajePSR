using PizzaPlanetaBiblioteca.Enums;

namespace PizzaPlanetaBiblioteca.DTOs.Pedidos;

public class PedidoDto
{
    public int Id { get; set; }

    public DateTime Fecha { get; set; }

    public EstadoPedido Estado { get; set; }

    public decimal Total { get; set; }

    public string Cliente { get; set; } = string.Empty;

    public List<ItemPedidoDto> Items { get; set; } = [];
}