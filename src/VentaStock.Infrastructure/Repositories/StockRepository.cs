using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using VentaStock.Domain.Contracts;
using VentaStock.Domain.Entities;
using VentaStock.Domain.Constants;
using VentaStock.Infrastructure.Data;

namespace VentaStock.Infrastructure.Repositories;

public class StockRepository(VentaStockDbContext context) : IStockRepository
{
    public async Task<IReadOnlyList<Branch>> GetBranchesAsync() =>
        await context.Branches.AsNoTracking().OrderBy(b => b.Name).ToListAsync();

    public async Task<IReadOnlyList<ProductStock>> GetAllByBranchAsync(int branchId, string? search, bool lowStockOnly)
    {
        var query = context.ProductStocks
            .Include(s => s.Product)
            .Include(s => s.Branch)
            .AsNoTracking()
            .Where(s => s.BranchId == branchId);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(s => s.Product.Name.Contains(search)
                || s.Product.Barcode.Contains(search)
                || (s.Sector != null && s.Sector.Contains(search))
                || (s.Shelf != null && s.Shelf.Contains(search)));
        if (lowStockOnly)
            query = query.Where(s => s.Quantity <= s.MinimumQuantity);
        return await query.OrderBy(s => s.Product.Name).ToListAsync();
    }

    public Task<ProductStock?> GetByProductAndBranchAsync(int productId, int branchId) => context.ProductStocks
        .Include(s => s.Product)
        .Include(s => s.Branch)
        .AsNoTracking()
        .FirstOrDefaultAsync(s => s.ProductId == productId && s.BranchId == branchId);

    public async Task<IReadOnlyList<StockMovement>> GetMovementsAsync(
        int productId,
        int branchId,
        DateOnly? fromDate,
        DateOnly? toDate,
        string? userName,
        string? movementType,
        int skip,
        int take)
    {
        var query = context.StockMovements
            .AsNoTracking()
            .Where(m => m.ProductId == productId && m.BranchId == branchId);

        if (fromDate.HasValue)
        {
            var startUtc = fromDate.Value.ToDateTime(TimeOnly.MinValue);
            query = query.Where(m => m.CreatedAtUtc >= startUtc);
        }

        if (toDate.HasValue)
        {
            var endExclusiveUtc = toDate.Value.AddDays(1).ToDateTime(TimeOnly.MinValue);
            query = query.Where(m => m.CreatedAtUtc < endExclusiveUtc);
        }

        if (!string.IsNullOrWhiteSpace(userName))
        {
            query = query.Where(m => m.UserName.Contains(userName));
        }

        if (!string.IsNullOrWhiteSpace(movementType))
        {
            query = query.Where(m => m.MovementType == movementType);
        }

        return await query
            .OrderByDescending(m => m.CreatedAtUtc)
            .ThenByDescending(m => m.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public Task<bool> ReceiveAsync(int productId, int branchId, int quantity, int userId, Guid operationId, string? reason) =>
        RecordAsync(productId, branchId, quantity, null, userId, operationId, reason, StockMovementTypes.Receipt);

    public Task<bool> AdjustAsync(int productId, int branchId, int countedQuantity, int expectedQuantity, int userId, Guid operationId, string reason) =>
        RecordAsync(productId, branchId, countedQuantity, expectedQuantity, userId, operationId, reason, StockMovementTypes.Adjustment);

    public async Task UpdateSettingsAsync(int productId, int branchId, string? sector, string? shelf, int minimumQuantity)
    {
        var stock = await context.ProductStocks
            .FirstOrDefaultAsync(s => s.ProductId == productId && s.BranchId == branchId)
            ?? throw new InvalidOperationException("El producto o la sucursal ya no existe o no tiene un registro de stock válido.");

        stock.Sector = sector;
        stock.Shelf = shelf;
        stock.MinimumQuantity = minimumQuantity;

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 547 })
        {
            throw new InvalidOperationException("No se pudo guardar la configuración: verificá los datos ingresados.", ex);
        }
    }

    private async Task<bool> RecordAsync(int productId, int branchId, int quantity, int? expectedQuantity, int userId,
        Guid operationId, string? reason, string movementType)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();
        var stock = await context.ProductStocks
            .FromSqlInterpolated($"SELECT * FROM dbo.ProductStocks WITH (UPDLOCK, HOLDLOCK) WHERE ProductId = {productId} AND BranchId = {branchId}")
            .SingleOrDefaultAsync()
            ?? throw new InvalidOperationException("El producto o la sucursal ya no existe o no tiene un registro de stock válido.");

        var previous = await context.StockMovements.AsNoTracking().SingleOrDefaultAsync(m => m.OperationId == operationId);
        if (previous is not null)
        {
            var sameQuantity = movementType == StockMovementTypes.Receipt
                ? previous.QuantityChange == quantity : previous.QuantityAfter == quantity;
            if (previous.ProductId != productId || previous.BranchId != branchId || previous.UserId != userId
                || previous.MovementType != movementType || previous.Reason != reason || !sameQuantity)
                throw new InvalidOperationException("Este formulario ya se utilizó con otros datos. Volvé a abrirlo.");
            await transaction.CommitAsync();
            return false;
        }

        if (expectedQuantity.HasValue && stock.Quantity != expectedQuantity.Value)
            throw new InvalidOperationException("Las existencias cambiaron mientras realizabas el conteo. Revisá el valor actualizado y volvé a confirmar la cantidad real.");

        var before = stock.Quantity;
        var after = movementType == StockMovementTypes.Receipt ? (long)before + quantity : quantity;
        if (after > int.MaxValue || after < int.MinValue)
            throw new InvalidOperationException("La operación supera el límite de unidades admitido.");
        var change = after - before;
        if (change == 0)
            throw new InvalidOperationException("La cantidad contada coincide con las existencias. No se realizó ningún ajuste.");

        var userName = await context.Users.Where(u => u.Id == userId).Select(u => u.UserName).SingleOrDefaultAsync()
            ?? throw new UnauthorizedAccessException();
        stock.Quantity = (int)after;
        context.StockMovements.Add(new StockMovement
        {
            ProductId = productId,
            BranchId = branchId,
            UserId = userId,
            UserName = userName,
            MovementType = movementType,
            QuantityChange = change,
            QuantityBefore = before,
            QuantityAfter = (int)after,
            Reason = reason,
            CreatedAtUtc = DateTime.UtcNow,
            OperationId = operationId
        });
        try
        {
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            throw new InvalidOperationException("El formulario ya se utilizó para otra operación. Volvé a abrirlo.", ex);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 547 })
        {
            throw new InvalidOperationException("No se pudo registrar el movimiento porque el producto, la sucursal o el usuario ya no está disponible.", ex);
        }
        return true;
    }
}
