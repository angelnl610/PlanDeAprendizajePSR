using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PizzaPlanetaApi.Modelos.Entidades
{

    public class Cliente
    {   
        public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Telefono { get; set; } = string.Empty;

        public string Direccion { get; set; } = string.Empty;
        public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
}
}