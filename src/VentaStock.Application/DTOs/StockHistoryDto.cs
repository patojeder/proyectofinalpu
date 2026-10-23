namespace VentaStock.Application.DTOs;

public class StockHistoryDto
{
    public StockItemDto Product { get; set; } = new();
    public List<StockMovementDto> Movements { get; set; } = [];
    public int Page { get; set; }
    public bool HasNextPage { get; set; }
}

public class StockMovementDto
{
    public long Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public long QuantityChange { get; set; }
    public int QuantityBefore { get; set; }
    public int QuantityAfter { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
