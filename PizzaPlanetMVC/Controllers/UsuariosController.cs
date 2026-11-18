using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PizzaPlanetaBiblioteca.DTOs;
using PizzaPlanetaBiblioteca.Enums;
using PizzaPlanetaBiblioteca.IServices;

namespace PizzaPlanetMVC.Controllers;

[Authorize(Roles = "Admin")]
public class UsuariosController : Controller
{
    private readonly IUsuarioService _usuarioService;

    public UsuariosController(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var usuarios = (await _usuarioService.ObtenerTodosAsync())
            .OrderBy(u => u.Id);
        return View(usuarios);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarRol(int id, RolUsuario rol)
    {
        var exito = await _usuarioService.CambiarRolAsync(id, new UsuarioRolActualizarDto { Rol = rol });
        if (exito)
        {
            TempData["MensajeExito"] = $"Rol de usuario #{id} reasignado a '{rol}' con éxito.";
        }
        else
        {
            TempData["MensajeError"] = $"No se pudo actualizar el rol del usuario #{id}.";
        }

        return RedirectToAction(nameof(Index));
    }
}
