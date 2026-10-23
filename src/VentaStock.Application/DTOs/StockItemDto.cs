namespace VentaStock.Application.DTOs;

public class StockItemDto
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public string? Sector { get; set; }
    public string? Shelf { get; set; }
    public int Quantity { get; set; }
    public int MinimumQuantity { get; set; }
}
