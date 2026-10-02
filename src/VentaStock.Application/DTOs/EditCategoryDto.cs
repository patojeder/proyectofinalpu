using System.ComponentModel.DataAnnotations;

namespace VentaStock.Application.DTOs;

public class EditCategoryDto
{
    [Required]
    public int IdCategoria { get; set; }

    [Required(ErrorMessage = "El nombre de la categoría es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }
}