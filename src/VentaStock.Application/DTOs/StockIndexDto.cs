namespace VentaStock.Application.DTOs;

public class StockIndexDto
{
    public string? Search { get; set; }
    public bool LowStockOnly { get; set; }
    public List<StockItemDto> Items { get; set; } = [];
}
