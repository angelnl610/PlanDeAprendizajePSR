namespace PizzaPlanetaApi.Modelos.DTOs.Pedidos;

public class CrearPedidoDto
{
    public int ClienteId { get; set; }

    public List<CrearItemPedidoDto> Items { get; set; } = [];
}