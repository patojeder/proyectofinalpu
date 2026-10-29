using System.ComponentModel.DataAnnotations;

namespace VentaStock.Application.DTOs;

public class StockSettingsDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Seleccioná un producto válido.")]
    public int ProductId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Seleccioná una sucursal válida.")]
    public int BranchId { get; set; }

    public StockItemDto? Product { get; set; }

    [StringLength(50, ErrorMessage = "El sector no puede superar los 50 caracteres.")]
    [Display(Name = "Sector de depósito")]
    public string? Sector { get; set; }

    [StringLength(50, ErrorMessage = "El estante no puede superar los 50 caracteres.")]
    [Display(Name = "Estante de depósito")]
    public string? Shelf { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "El stock mínimo debe ser un entero mayor o igual a 0.")]
    [Display(Name = "Stock mínimo")]
    public int? MinimumQuantity { get; set; } = 0;
}

