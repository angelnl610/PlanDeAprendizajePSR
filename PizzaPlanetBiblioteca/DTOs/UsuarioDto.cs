using PizzaPlanetaBiblioteca.Enums;

namespace PizzaPlanetaBiblioteca.DTOs;

public class UsuarioDetalleDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public RolUsuario Rol { get; set; }
    public string Telefono { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
}

public class UsuarioRolActualizarDto
{
    public RolUsuario Rol { get; set; }
}
