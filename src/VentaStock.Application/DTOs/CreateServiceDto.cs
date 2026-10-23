using System.ComponentModel.DataAnnotations;

namespace VentaStock.Application.DTOs;

public class CreateServiceDto
{
    [Required(ErrorMessage = "El nombre del servicio es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    [Display(Name = "Nombre")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Descripción")]
    public string? Description { get; set; }

    [Display(Name = "Categorías")]
    public List<int> CategoryIds { get; set; } = [];
}
