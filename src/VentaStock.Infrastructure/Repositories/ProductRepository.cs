using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using VentaStock.Domain.Contracts;
using VentaStock.Domain.Entities;
using VentaStock.Infrastructure.Data;

namespace VentaStock.Infrastructure.Repositories;

public class ProductRepository(VentaStockDbContext context) : IProductRepository
{
    public async Task<IEnumerable<Product>> GetAllAsync(string? searchTerm = null)
    {
        IQueryable<Product> query = context.Products
            .Include(p => p.Categories)
            .Include(p => p.ProductStocks)
            .AsNoTracking();
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim();
            query = query.Where(p => p.Name.Contains(term)
                || p.Barcode.Contains(term)
                || (p.Description != null && p.Description.Contains(term))
                || p.Categories.Any(c => c.Name.Contains(term)));
        }

        return await query.OrderBy(p => p.Name).ToListAsync();
    }

    public Task<Product?> GetByIdAsync(int id) => context.Products
        .Include(p => p.Categories)
        .Include(p => p.ProductStocks)
        .FirstOrDefaultAsync(p => p.Id == id);

    public Task<bool> ExistsByNameAsync(string name, int? excludeId = null) => context.Products
        .AnyAsync(p => p.Name == name && (!excludeId.HasValue || p.Id != excludeId.Value));

    public Task<bool> ExistsByBarcodeAsync(string barcode, int? excludeId = null) => context.Products
        .AnyAsync(p => p.Barcode == barcode && (!excludeId.HasValue || p.Id != excludeId.Value));

    public async Task CreateAsync(Product product)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();
        var branchIds = await context.Branches.Select(b => b.Id).ToListAsync();
        foreach (var branchId in branchIds)
        {
            product.ProductStocks.Add(new ProductStock
            {
                BranchId = branchId,
                Quantity = 0,
                MinimumQuantity = 0
            });
        }

        context.Products.Add(product);
        await SaveAsync();
        await transaction.CommitAsync();
    }

    public Task UpdateAsync(Product product) => SaveAsync();

    public async Task DeleteAsync(int id)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();
        var stocks = await context.ProductStocks
            .FromSqlInterpolated($"SELECT * FROM dbo.ProductStocks WITH (UPDLOCK, HOLDLOCK) WHERE ProductId = {id}")
            .ToListAsync();
        var product = await context.Products.FindAsync(id)
            ?? throw new InvalidOperationException("El producto ya no existe.");
        if (await context.StockMovements.AnyAsync(m => m.ProductId == id))
            throw new InvalidOperationException("No se puede eliminar un producto con historial de movimientos de stock.");
        if (stocks.Any(s => s.Quantity != 0))
            throw new InvalidOperationException("No se puede eliminar un producto con existencias positivas o negativas en alguna sucursal.");
        context.Products.Remove(product);
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
            throw new InvalidOperationException("El producto cambió o fue eliminado. Recargá la pantalla.", ex);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            throw new InvalidOperationException("Ya existe un producto con ese nombre o código de barras.", ex);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 547 })
        {
            throw new InvalidOperationException("No se pudo completar la operación: hay registros relacionados o una categoría ya no está disponible.", ex);
        }
    }
}
