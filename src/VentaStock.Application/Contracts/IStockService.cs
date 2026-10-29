using VentaStock.Application.DTOs;

namespace VentaStock.Application.Contracts;

public interface IStockService
{
    Task<StockIndexDto> GetAllAsync(int? branchId, string? search, bool lowStockOnly);
    Task<StockReceiptDto?> GetForReceiptAsync(int productId, int branchId);
    Task<StockAdjustmentDto?> GetForAdjustmentAsync(int productId, int branchId);
    Task<StockSettingsDto?> GetForSettingsAsync(int productId, int branchId);
    Task<StockHistoryDto?> GetHistoryAsync(int productId, int branchId, StockHistoryFilterDto? filter, int page);
    Task<bool> ReceiveAsync(StockReceiptDto dto);
    Task<bool> AdjustAsync(StockAdjustmentDto dto);
    Task UpdateSettingsAsync(StockSettingsDto dto);
}
