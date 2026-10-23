using System.ComponentModel.DataAnnotations;

namespace VentaStock.Application.DTOs;

public class CreateProductDto : ProductFieldsDto
{
    [Display(Name = "Costo")]
    public decimal? CostPrice { get; set; }

    [Display(Name = "Precio de venta")]
    public decimal? SalePrice { get; set; }
}
