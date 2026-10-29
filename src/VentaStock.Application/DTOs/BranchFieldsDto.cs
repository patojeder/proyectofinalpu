using System.ComponentModel.DataAnnotations;

namespace VentaStock.Application.DTOs;

public class BranchFieldsDto
{
    [Required(ErrorMessage = "El nombre de la sucursal es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    [Display(Name = "Nombre")]
    public string Name { get; set; } = string.Empty;

    [StringLength(250, ErrorMessage = "La dirección no puede superar los 250 caracteres.")]
    [Display(Name = "Dirección")]
    public string? Address { get; set; }

    [StringLength(50, ErrorMessage = "El teléfono no puede superar los 50 caracteres.")]
    [Display(Name = "Teléfono")]
    public string? Phone { get; set; }
}

