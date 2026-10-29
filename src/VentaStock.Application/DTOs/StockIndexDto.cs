namespace VentaStock.Application.DTOs;

public class StockIndexDto
{
    public int? BranchId { get; set; }
    public string? Search { get; set; }
    public bool LowStockOnly { get; set; }
    public List<BranchSelectDto> Branches { get; set; } = [];
    public List<StockItemDto> Items { get; set; } = [];
}
