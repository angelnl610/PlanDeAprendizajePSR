using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PizzaPlanetaBiblioteca.DTOs;
using PizzaPlanetaBiblioteca.IServices;

namespace PizzaPlanetMVC.Controllers;

[Authorize]
public class PizzasController : Controller
{
    private readonly IPizzaService _pizzaService;

    public PizzasController(IPizzaService pizzaService)
    {
        _pizzaService = pizzaService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var pizzas = await _pizzaService.ObtenerTodasAsync();
        return View(pizzas);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Cocinero")]
    public IActionResult Crear()
    {
        return View(new PizzaAltaDto());
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Cocinero")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(PizzaAltaDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre) || dto.Precio <= 0)
        {
            ModelState.AddModelError(string.Empty, "Nombre obligatorio y precio mayor a 0.");
            return View(dto);
        }

        var nuevaPizza = await _pizzaService.CrearAsync(dto);
        if (nuevaPizza is null)
        {
            ModelState.AddModelError(string.Empty, "No se pudo registrar la pizza en la API.");
            return View(dto);
        }

        TempData["MensajeExito"] = $"Pizza '{dto.Nombre}' incorporada al catálogo exitosamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Cocinero")]
    public async Task<IActionResult> Editar(int id)
    {
        var pizza = await _pizzaService.ObtenerPorIdAsync(id);
        if (pizza is null)
        {
            return NotFound();
        }

        var dto = new PizzaActualizarDto
        {
            Nombre = pizza.Nombre,
            Descripcion = pizza.Descripcion,
            Precio = pizza.Precio,
            ImagenUrl = pizza.ImagenUrl
        };

        ViewData["PizzaId"] = id;
        return View(dto);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Cocinero")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, PizzaActualizarDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre) || dto.Precio <= 0)
        {
            ViewData["PizzaId"] = id;
            ModelState.AddModelError(string.Empty, "Nombre obligatorio y precio mayor a 0.");
            return View(dto);
        }

        var exito = await _pizzaService.ActualizarAsync(id, dto);
        if (!exito)
        {
            ViewData["PizzaId"] = id;
            ModelState.AddModelError(string.Empty, "No se pudo actualizar la pizza.");
            return View(dto);
        }

        TempData["MensajeExito"] = $"Pizza '{dto.Nombre}' actualizada exitosamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Cocinero")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        var exito = await _pizzaService.EliminarAsync(id);
        if (exito)
        {
            TempData["MensajeExito"] = "Pizza retirada del catálogo activo con éxito (Soft Delete).";
        }
        else
        {
            TempData["MensajeError"] = "No se pudo retirar la pizza del catálogo.";
        }

        return RedirectToAction(nameof(Index));
    }
}
