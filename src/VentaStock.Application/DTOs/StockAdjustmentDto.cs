using System.ComponentModel.DataAnnotations;

namespace VentaStock.Application.DTOs;

public class StockAdjustmentDto : StockOperationDto
{
    [Required(ErrorMessage = "Indicá la cantidad real contada.")]
    [Range(0, int.MaxValue, ErrorMessage = "La cantidad contada debe ser un entero mayor o igual a 0.")]
    [Display(Name = "Cantidad real contada")]
    public int? CountedQuantity { get; set; }

    [Required(ErrorMessage = "Recargá la pantalla antes de confirmar el ajuste.")]
    public int? ExpectedQuantity { get; set; }

    [Required(ErrorMessage = "Indicá el motivo del ajuste.")]
    [StringLength(500, ErrorMessage = "El motivo no puede superar los 500 caracteres.")]
    [Display(Name = "Motivo")]
    public string Reason { get; set; } = string.Empty;
}
