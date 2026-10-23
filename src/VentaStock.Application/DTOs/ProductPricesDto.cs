using System.ComponentModel.DataAnnotations;

namespace VentaStock.Application.DTOs;

public class ProductPricesDto
{
    [Range(1, int.MaxValue)]
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    [Display(Name = "Costo")]
    public decimal? CostPrice { get; set; }

    [Display(Name = "Precio de venta")]
    public decimal? SalePrice { get; set; }
}
