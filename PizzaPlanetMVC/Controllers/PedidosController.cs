using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PizzaPlanetaBiblioteca.DTOs;
using PizzaPlanetaBiblioteca.IServices;
using PizzaPlanetMVC.Models;

namespace PizzaPlanetMVC.Controllers;

[Authorize(Roles = "Admin,Cliente")]
public class PedidosController : Controller
{
    private readonly IPedidoService _pedidoService;
    private readonly IPizzaService _pizzaService;

    public PedidosController(IPedidoService pedidoService, IPizzaService pizzaService)
    {
        _pedidoService = pedidoService;
        _pizzaService = pizzaService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var pedidos = await _pedidoService.ObtenerTodosAsync();
        return View(pedidos);
    }

    [HttpGet]
    public async Task<IActionResult> Crear(int? pizzaId = null)
    {
        var pizzas = await _pizzaService.ObtenerTodasAsync();
        var model = new PedidoCreacionViewModel
        {
            Pizzas = pizzas.Select(p => new ItemSeleccionPizzaViewModel
            {
                PizzaId = p.Id,
                Nombre = p.Nombre,
                Precio = p.Precio,
                Cantidad = (pizzaId.HasValue && pizzaId.Value == p.Id) ? 1 : 0
            }).ToList()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(PedidoCreacionViewModel model)
    {
        // Asegurar que nombres y precios estén presentes para renderizado en caso de error
        var catalogo = (await _pizzaService.ObtenerTodasAsync()).ToDictionary(p => p.Id);
        if (model.Pizzas == null || model.Pizzas.Count == 0)
        {
            model.Pizzas = catalogo.Values.Select(p => new ItemSeleccionPizzaViewModel
            {
                PizzaId = p.Id,
                Nombre = p.Nombre,
                Precio = p.Precio,
                Cantidad = 0
            }).ToList();
        }
        else
        {
            foreach (var item in model.Pizzas)
            {
                if (catalogo.TryGetValue(item.PizzaId, out var pizza))
                {
                    item.Nombre = pizza.Nombre;
                    item.Precio = pizza.Precio;
                }
            }
        }

        // Validación defensiva de cantidades negativas
        if (model.Pizzas.Any(p => p.Cantidad < 0))
        {
            ModelState.AddModelError(string.Empty, "Las cantidades no pueden ser negativas.");
            return View(model);
        }

        var itemsSeleccionados = model.Pizzas.Where(p => p.Cantidad > 0).ToList();
        if (itemsSeleccionados.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "Debe seleccionar al menos una pizza con cantidad mayor a cero.");
            return View(model);
        }

        var dto = new PedidoAltaDto
        {
            Items = itemsSeleccionados.Select(i => new ItemPedidoAltaDto
            {
                PizzaId = i.PizzaId,
                Cantidad = i.Cantidad
            }).ToList()
        };

        var nuevoPedido = await _pedidoService.CrearAsync(dto);
        if (nuevoPedido is null)
        {
            ModelState.AddModelError(string.Empty, "No se pudo registrar el pedido en la API o una o más pizzas no están disponibles.");
            return View(model);
        }

        TempData["MensajeExito"] = $"¡Pedido #{nuevoPedido.Id} confirmado con éxito!";
        return RedirectToAction(nameof(Detalle), new { id = nuevoPedido.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Detalle(int id)
    {
        var pedido = await _pedidoService.ObtenerPorIdAsync(id);
        if (pedido is null)
        {
            return NotFound();
        }

        return View(pedido);
    }
}
