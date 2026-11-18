using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PizzaPlanetaBiblioteca.DTOs;
using PizzaPlanetaBiblioteca.Enums;
using PizzaPlanetaBiblioteca.IServices;

namespace PizzaPlanetMVC.Controllers;

public class CuentaController : Controller
{
    private readonly IAuthService _authService;

    public CuentaController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirigirPorRol();
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginDto());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginDto dto, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        var authRespuesta = await _authService.LoginAsync(dto);
        if (authRespuesta is null)
        {
            ModelState.AddModelError(string.Empty, "Credenciales inválidas. Verifique su correo y contraseña.");
            return View(dto);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, authRespuesta.Email),
            new(ClaimTypes.Name, authRespuesta.Nombre),
            new(ClaimTypes.Email, authRespuesta.Email),
            new(ClaimTypes.Role, authRespuesta.Rol.ToString()),
            new("TokenJwt", authRespuesta.Token)
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirigirPorRol();
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Registro()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirigirPorRol();
        }

        return View(new RegistroDto());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Registro(RegistroDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        var exito = await _authService.RegistroAsync(dto);
        if (!exito)
        {
            ModelState.AddModelError(string.Empty, "No se pudo completar el registro. Verifique que el correo no se encuentre ya en uso.");
            return View(dto);
        }

        TempData["MensajeExito"] = "¡Registro completado exitosamente! Ya puede iniciar sesión.";
        return RedirectToAction(nameof(Login));
    }

    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    public IActionResult AccesoDenegado()
    {
        return View();
    }

    private IActionResult RedirigirPorRol()
    {
        if (User.IsInRole(nameof(RolUsuario.Admin)))
        {
            return RedirectToAction("Index", "Usuarios");
        }

        return RedirectToAction("Index", "Pizzas");
    }
}
