using PizzaPlanetaBiblioteca.Enums;

namespace PizzaPlanetaBiblioteca.Entidades;

public class Pedido
{
    public int Id { get; set; }

    public DateTime Fecha { get; set; } = DateTime.Now;

    public EstadoPedido Estado { get; set; } = EstadoPedido.Pendiente;

    public decimal Total { get; set; }

    public int ClienteId { get; set; }

    public Cliente Cliente { get; set; } = null!;

    public ICollection<ItemPedido> ItemsPedido { get; set; } = new List<ItemPedido>();
}