using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using VentaStock.Domain.Contracts;
using VentaStock.Domain.Entities;
using VentaStock.Domain.Constants;
using VentaStock.Infrastructure.Data;

namespace VentaStock.Infrastructure.Repositories;

public class StockRepository(VentaStockDbContext context) : IStockRepository
{
    public async Task<IReadOnlyList<ProductStock>> GetAllAsync(string? search, bool lowStockOnly)
    {
        var query = context.ProductStocks.Include(s => s.Product).AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(s => s.Product.Name.Contains(search) || s.Product.Barcode.Contains(search));
        if (lowStockOnly)
            query = query.Where(s => s.Quantity <= s.MinimumQuantity);
        return await query.OrderBy(s => s.Product.Name).ToListAsync();
    }

    public Task<ProductStock?> GetByProductIdAsync(int productId) => context.ProductStocks
        .Include(s => s.Product).AsNoTracking().FirstOrDefaultAsync(s => s.ProductId == productId);

    public async Task<IReadOnlyList<StockMovement>> GetMovementsAsync(int productId, int skip, int take) =>
        await context.StockMovements.AsNoTracking().Where(m => m.ProductId == productId)
            .OrderByDescending(m => m.CreatedAtUtc).ThenByDescending(m => m.Id)
            .Skip(skip).Take(take).ToListAsync();

    public Task<bool> ReceiveAsync(int productId, int quantity, int userId, Guid operationId, string? reason) =>
        RecordAsync(productId, quantity, null, userId, operationId, reason, StockMovementTypes.Receipt);

    public Task<bool> AdjustAsync(int productId, int countedQuantity, int expectedQuantity, int userId, Guid operationId, string reason) =>
        RecordAsync(productId, countedQuantity, expectedQuantity, userId, operationId, reason, StockMovementTypes.Adjustment);

    private async Task<bool> RecordAsync(int productId, int quantity, int? expectedQuantity, int userId,
        Guid operationId, string? reason, string movementType)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();
        var stock = await context.ProductStocks
            .FromSqlInterpolated($"SELECT * FROM dbo.ProductStocks WITH (UPDLOCK, HOLDLOCK) WHERE ProductId = {productId}")
            .SingleOrDefaultAsync()
            ?? throw new InvalidOperationException("El producto ya no existe o no tiene un registro de stock válido.");

        var previous = await context.StockMovements.AsNoTracking().SingleOrDefaultAsync(m => m.OperationId == operationId);
        if (previous is not null)
        {
            var sameQuantity = movementType == StockMovementTypes.Receipt
                ? previous.QuantityChange == quantity : previous.QuantityAfter == quantity;
            if (previous.ProductId != productId || previous.UserId != userId || previous.MovementType != movementType
                || previous.Reason != reason || !sameQuantity)
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
            ProductId = productId, UserId = userId, UserName = userName,
            MovementType = movementType, QuantityChange = change, QuantityBefore = before,
            QuantityAfter = (int)after, Reason = reason, CreatedAtUtc = DateTime.UtcNow, OperationId = operationId
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
            throw new InvalidOperationException("No se pudo registrar el movimiento porque el producto o usuario ya no está disponible.", ex);
        }
        return true;
    }
}
