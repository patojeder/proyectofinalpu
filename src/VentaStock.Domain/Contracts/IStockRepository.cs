using VentaStock.Domain.Entities;

namespace VentaStock.Domain.Contracts;

public interface IStockRepository
{
    Task<IReadOnlyList<ProductStock>> GetAllAsync(string? search, bool lowStockOnly);
    Task<ProductStock?> GetByProductIdAsync(int productId);
    Task<IReadOnlyList<StockMovement>> GetMovementsAsync(int productId, int skip, int take);
    Task<bool> ReceiveAsync(int productId, int quantity, int userId, Guid operationId, string? reason);
    Task<bool> AdjustAsync(int productId, int countedQuantity, int expectedQuantity, int userId, Guid operationId, string reason);
}
