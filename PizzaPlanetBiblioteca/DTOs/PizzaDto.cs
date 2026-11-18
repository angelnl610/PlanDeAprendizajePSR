using System.ComponentModel.DataAnnotations;

namespace PizzaPlanetaBiblioteca.DTOs;

public class PizzaDetalleDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public string? ImagenUrl { get; set; }
    public bool Activa { get; set; } = true;
}

public class PizzaAltaDto
{
    [Required(ErrorMessage = "El nombre de la pizza es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(300, ErrorMessage = "La descripción no puede superar los 300 caracteres.")]
    public string? Descripcion { get; set; } = string.Empty;

    [Required(ErrorMessage = "El precio es obligatorio.")]
    [Range(0.01, 10000000.0, ErrorMessage = "El precio debe ser mayor a 0.")]
    public decimal Precio { get; set; }

    [StringLength(500, ErrorMessage = "La URL de la imagen no puede superar los 500 caracteres.")]
    public string? ImagenUrl { get; set; }
}

public class PizzaActualizarDto
{
    [Required(ErrorMessage = "El nombre de la pizza es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(300, ErrorMessage = "La descripción no puede superar los 300 caracteres.")]
    public string? Descripcion { get; set; } = string.Empty;

    [Required(ErrorMessage = "El precio es obligatorio.")]
    [Range(0.01, 10000000.0, ErrorMessage = "El precio debe ser mayor a 0.")]
    public decimal Precio { get; set; }

    [StringLength(500, ErrorMessage = "La URL de la imagen no puede superar los 500 caracteres.")]
    public string? ImagenUrl { get; set; }
}

public class PizzaEliminarDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
}
