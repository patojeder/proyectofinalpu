using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using VentaStock.Domain.Contracts;
using VentaStock.Domain.Entities;
using VentaStock.Infrastructure.Data;

namespace VentaStock.Infrastructure.Repositories;

public class BranchRepository(VentaStockDbContext context) : IBranchRepository
{
    public async Task<IEnumerable<Branch>> GetAllAsync(string? searchTerm = null)
    {
        IQueryable<Branch> query = context.Branches.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim();
            query = query.Where(b => b.Name.Contains(term)
                || (b.Address != null && b.Address.Contains(term))
                || (b.Phone != null && b.Phone.Contains(term)));
        }

        return await query.OrderBy(b => b.Name).ToListAsync();
    }

    public Task<Branch?> GetByIdAsync(int id) =>
        context.Branches.FirstOrDefaultAsync(b => b.Id == id);

    public Task<List<Branch>> GetByIdsAsync(IEnumerable<int> ids) =>
        context.Branches.Where(b => ids.Contains(b.Id)).ToListAsync();

    public Task<bool> ExistsByNameAsync(string name, int? excludeId = null) =>
        context.Branches.AnyAsync(b => b.Name == name && (!excludeId.HasValue || b.Id != excludeId.Value));

    public async Task CreateAsync(Branch branch)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();
        var productIds = await context.Products.Select(p => p.Id).ToListAsync();
        foreach (var productId in productIds)
        {
            branch.ProductStocks.Add(new ProductStock
            {
                ProductId = productId,
                Quantity = 0,
                MinimumQuantity = 0
            });
        }

        context.Branches.Add(branch);
        await SaveAsync();
        await transaction.CommitAsync();
    }

    public Task UpdateAsync(Branch branch) => SaveAsync();

    public async Task DeleteAsync(int id)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();
        var stocks = await context.ProductStocks
            .FromSqlInterpolated($"SELECT * FROM dbo.ProductStocks WITH (UPDLOCK, HOLDLOCK) WHERE BranchId = {id}")
            .ToListAsync();
        var branch = await context.Branches.FindAsync(id)
            ?? throw new InvalidOperationException("La sucursal ya no existe.");
        if (await context.StockMovements.AnyAsync(m => m.BranchId == id))
            throw new InvalidOperationException("No se puede eliminar una sucursal con historial de movimientos de stock.");
        if (stocks.Any(s => s.Quantity != 0))
            throw new InvalidOperationException("No se puede eliminar una sucursal con existencias positivas o negativas.");

        context.Branches.Remove(branch);
        await SaveAsync();
        await transaction.CommitAsync();
    }

    private async Task SaveAsync()
    {
        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new InvalidOperationException("La sucursal cambió o fue eliminada. Recargá la pantalla.", ex);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            throw new InvalidOperationException("Ya existe una sucursal con ese nombre.", ex);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 547 })
        {
            throw new InvalidOperationException("No se pudo completar la operación porque la sucursal tiene registros asociados.", ex);
        }
    }
}
