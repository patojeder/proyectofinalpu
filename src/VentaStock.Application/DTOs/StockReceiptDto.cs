using System.ComponentModel.DataAnnotations;

namespace VentaStock.Application.DTOs;

public class StockReceiptDto : StockOperationDto
{
    [Required(ErrorMessage = "Indicá la cantidad a ingresar.")]
    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser un entero mayor a 0.")]
    [Display(Name = "Cantidad a ingresar")]
    public int? Quantity { get; set; }

    [StringLength(500, ErrorMessage = "La observación no puede superar los 500 caracteres.")]
    [Display(Name = "Observación (opcional)")]
    public string? Reason { get; set; }
}
