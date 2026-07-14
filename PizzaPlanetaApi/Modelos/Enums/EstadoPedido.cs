using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PizzaPlanetaApi.Modelos.Enums
{
    public enum EstadoPedido
    {
        Pendiente = 1,
        EnPreparacion = 2,
        EnCamino = 3,
        Entregado = 4,
        Cancelado = 5
    }
}