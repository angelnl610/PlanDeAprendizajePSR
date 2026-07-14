using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PizzaPlanetaApi.Modelos.Entidades
{
    public class ItemPedido
    {
        public int Id { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal => Cantidad * PrecioUnitario;
        public int PedidoId { get; set; }
        public Pedido Pedido { get; set; } = null!;
        public int PizzaId { get; set; }   
        public Pizza Pizza { get; set; } = null!;
    }
}