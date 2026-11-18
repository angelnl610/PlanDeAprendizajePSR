using PizzaPlanetaBiblioteca.Enums;

namespace PizzaPlanetaBiblioteca.Entidades;

public class Usuario
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public RolUsuario Rol { get; set; } = RolUsuario.Cliente;
    public string Telefono { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;

    public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
}
