using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PizzaPlanetaApi.Modelos.Entidades
{
    public class Pizza
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        public decimal Precio { get; set; }

        public ICollection<ItemPedido> ItemsPedido { get; set; } = new List<ItemPedido>();
    }
}