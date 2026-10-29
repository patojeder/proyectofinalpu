using System.ComponentModel.DataAnnotations;

namespace VentaStock.Application.DTOs;

public class ProductFieldsDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(150, ErrorMessage = "El nombre no puede superar los 150 caracteres.")]
    [Display(Name = "Nombre")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Descripción")]
    public string? Description { get; set; }

    [StringLength(100, ErrorMessage = "El código no puede superar los 100 caracteres.")]
    [Display(Name = "Código de barras")]
    public string? Barcode { get; set; }

    [MinLength(1, ErrorMessage = "Seleccioná al menos una categoría.")]
    [Display(Name = "Categorías")]
    public List<int> CategoryIds { get; set; } = [];
}
