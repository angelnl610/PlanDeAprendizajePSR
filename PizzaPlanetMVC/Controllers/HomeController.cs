using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PizzaPlanetaBiblioteca.DTOs.Pizzas;
using PizzaPlanetMVC.Models;
using PizzaPlanetMVC.Services;

namespace PizzaPlanetMVC.Controllers;

public class HomeController : Controller
{
    private readonly IPizzaApiService _pizzaApiService;
    private readonly ILogger<HomeController> _logger;

    public HomeController(IPizzaApiService pizzaApiService, ILogger<HomeController> logger)
    {
        _pizzaApiService = pizzaApiService;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Pizzas([FromQuery] string? searchTerm, [FromQuery] string? sortOrder)
    {
        var viewModel = new PizzaCatalogoViewModel
        {
            SearchTerm = searchTerm,
            SortOrder = sortOrder
        };

        try
        {
            // Consulta 100% asíncrona a la API REST
            var pizzasDto = await _pizzaApiService.ObtenerPizzasAsync();

            var query = pizzasDto.AsEnumerable();

            // Filtrado por buscador HUD
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLowerInvariant();
                query = query.Where(p => 
                    (p.Nombre != null && p.Nombre.ToLowerInvariant().Contains(term)) ||
                    (p.Descripcion != null && p.Descripcion.ToLowerInvariant().Contains(term)));
            }

            // Ordenamiento por telemetría de costos
            query = sortOrder switch
            {
                "asc" => query.OrderBy(p => p.Precio),
                "desc" => query.OrderByDescending(p => p.Precio),
                _ => query
            };

            viewModel.Pizzas = query.Select(p => new PizzaItemViewModel
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                Precio = p.Precio,
                ImagenUrl = p.ImagenUrl
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error asíncrono al cargar el catálogo de pizzas desde la API.");
            viewModel.HasError = true;
            viewModel.ErrorMessage = "No se pudo conectar con la estación central de Pizza Planeta. Intente nuevamente.";
        }

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearPizza(CrearPizzaDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre) || dto.Precio <= 0)
        {
            TempData["MensajeError"] = "Parámetros inválidos para crear la pizza espacial.";
            return RedirectToAction(nameof(Pizzas));
        }

        var exito = await _pizzaApiService.CrearPizzaAsync(dto);
        if (exito)
        {
            TempData["MensajeExito"] = $"¡Pizza '{dto.Nombre}' incorporada al catálogo galáctico con éxito!";
        }
        else
        {
            TempData["MensajeError"] = "No se pudo crear la pizza en la estación central. Intente nuevamente.";
        }

        return RedirectToAction(nameof(Pizzas));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarPizza(int id, ActualizarPizzaDto dto)
    {
        if (id <= 0 || string.IsNullOrWhiteSpace(dto.Nombre) || dto.Precio <= 0)
        {
            TempData["MensajeError"] = "Parámetros inválidos para actualizar la pizza espacial.";
            return RedirectToAction(nameof(Pizzas));
        }

        var exito = await _pizzaApiService.ActualizarPizzaAsync(id, dto);
        if (exito)
        {
            TempData["MensajeExito"] = $"¡Parámetros de la pizza '{dto.Nombre}' actualizados con éxito!";
        }
        else
        {
            TempData["MensajeError"] = $"No se pudo actualizar la pizza con ID #{id} en la API.";
        }

        return RedirectToAction(nameof(Pizzas));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarPizza(int id)
    {
        if (id <= 0)
        {
            TempData["MensajeError"] = "Identificador de pizza inválido para dar de baja.";
            return RedirectToAction(nameof(Pizzas));
        }

        var exito = await _pizzaApiService.EliminarPizzaAsync(id);
        if (exito)
        {
            TempData["MensajeExito"] = $"¡La pizza con ID #{id} ha sido dada de baja del sector interestelar!";
        }
        else
        {
            TempData["MensajeError"] = $"No se pudo dar de baja la pizza #{id}. Verifique si posee pedidos asociados en el reactor.";
        }

        return RedirectToAction(nameof(Pizzas));
    }

    [HttpGet]
    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
