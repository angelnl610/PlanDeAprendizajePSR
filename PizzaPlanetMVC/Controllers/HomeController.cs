using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PizzaPlanetMVC.Models;

namespace PizzaPlanetMVC.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return RedirectToAction("Login", "Cuenta");
        }

        if (User.IsInRole("Admin"))
        {
            return RedirectToAction("Index", "Usuarios");
        }

        return RedirectToAction("Index", "Pizzas");
    }

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
