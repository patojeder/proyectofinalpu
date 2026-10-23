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

    [StringLength(50, ErrorMessage = "El sector no puede superar los 50 caracteres.")]
    [Display(Name = "Sector")]
    public string? Sector { get; set; }

    [StringLength(50, ErrorMessage = "El estante no puede superar los 50 caracteres.")]
    [Display(Name = "Estante")]
    public string? Shelf { get; set; }

    [MinLength(1, ErrorMessage = "Seleccioná al menos una categoría.")]
    [Display(Name = "Categorías")]
    public List<int> CategoryIds { get; set; } = [];

    [Range(0, int.MaxValue, ErrorMessage = "El stock mínimo debe ser un entero mayor o igual a 0.")]
    [Display(Name = "Stock mínimo")]
    public int? MinimumQuantity { get; set; } = 0;
}
