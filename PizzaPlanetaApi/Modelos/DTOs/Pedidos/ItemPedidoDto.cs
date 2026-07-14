namespace PizzaPlanetaApi.Modelos.DTOs.Pedidos;

public class ItemPedidoDto
{
    public int PizzaId { get; set; }

    public string Pizza { get; set; } = string.Empty;

    public int Cantidad { get; set; }

    public decimal PrecioUnitario { get; set; }

    public decimal Subtotal => Cantidad * PrecioUnitario;
}