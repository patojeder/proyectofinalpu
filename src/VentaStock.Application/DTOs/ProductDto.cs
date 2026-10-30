namespace VentaStock.Application.DTOs;

public class ProductBranchStockDto
{
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public int Quantity { get; set; }
}

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Barcode { get; set; } = string.Empty;
    public List<string> Categories { get; set; } = [];
    public decimal? CostPrice { get; set; }
    public decimal? SalePrice { get; set; }
    public int? Quantity { get; set; }
    public List<ProductBranchStockDto> BranchStocks { get; set; } = [];
}
