using System.ComponentModel.DataAnnotations;

namespace VentaStock.Application.DTOs;

public abstract class StockOperationDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Seleccioná un producto válido.")]
    public int ProductId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Seleccioná una sucursal válida.")]
    public int BranchId { get; set; }

    public Guid OperationId { get; set; }
    public StockItemDto? Product { get; set; }
}
