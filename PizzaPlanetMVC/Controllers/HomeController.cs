using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
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
    public async Task<IActionResult> Index([FromQuery] string? searchTerm, [FromQuery] string? sortOrder)
    {
        var viewModel = new PizzaCatalogoViewModel
        {
            SearchTerm = searchTerm,
            SortOrder = sortOrder
        };

        try
        {
            // Consulta 100% asíncrona a la API
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
                ImagenUrl = PizzaItemViewModel.ResolverImagen(p.Nombre)
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
