using VentaStock.Domain.Entities;

namespace VentaStock.Domain.Contracts;

public interface IStockRepository
{
    Task<IReadOnlyList<Branch>> GetBranchesAsync();
    Task<IReadOnlyList<ProductStock>> GetAllByBranchAsync(int branchId, string? search, bool lowStockOnly);
    Task<ProductStock?> GetByProductAndBranchAsync(int productId, int branchId);
    Task<IReadOnlyList<StockMovement>> GetMovementsAsync(
        int productId,
        int branchId,
        DateOnly? fromDate,
        DateOnly? toDate,
        string? userName,
        string? movementType,
        int skip,
        int take);
    Task<bool> ReceiveAsync(int productId, int branchId, int quantity, int userId, Guid operationId, string? reason);
    Task<bool> AdjustAsync(int productId, int branchId, int countedQuantity, int expectedQuantity, int userId, Guid operationId, string reason);
    Task UpdateSettingsAsync(int productId, int branchId, string? sector, string? shelf, int minimumQuantity);
}
