using System.ComponentModel.DataAnnotations;
using PizzaPlanetaBiblioteca.Enums;

namespace PizzaPlanetaBiblioteca.DTOs;

public class LoginDto
{
    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "El formato de correo no es válido.")]
    [StringLength(150, ErrorMessage = "El correo no puede superar los 150 caracteres.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    public string Password { get; set; } = string.Empty;
}

public class RegistroDto
{
    [Required(ErrorMessage = "El nombre y apellido son obligatorios.")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "El formato de correo no es válido.")]
    [StringLength(150, ErrorMessage = "El correo no puede superar los 150 caracteres.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 100 caracteres.")]
    public string Password { get; set; } = string.Empty;

    [StringLength(30, ErrorMessage = "El teléfono no puede superar los 30 caracteres.")]
    public string Telefono { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "La dirección no puede superar los 200 caracteres.")]
    public string Direccion { get; set; } = string.Empty;
}

public class AuthRespuestaDto
{
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public RolUsuario Rol { get; set; }
}
