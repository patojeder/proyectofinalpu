using VentaStock.Application.DTOs;

namespace VentaStock.Application.Contracts;

public interface IStockService
{
    Task<StockIndexDto> GetAllAsync(string? search, bool lowStockOnly);
    Task<StockReceiptDto?> GetForReceiptAsync(int productId);
    Task<StockAdjustmentDto?> GetForAdjustmentAsync(int productId);
    Task<StockHistoryDto?> GetHistoryAsync(int productId, int page);
    Task<bool> ReceiveAsync(StockReceiptDto dto);
    Task<bool> AdjustAsync(StockAdjustmentDto dto);
}
