using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using VentaStock.Domain.Contracts;
using VentaStock.Domain.Entities;
using VentaStock.Infrastructure.Data;

namespace VentaStock.Infrastructure.Repositories;

public class ProductRepository(VentaStockDbContext context) : IProductRepository
{
    public async Task<IEnumerable<Product>> GetAllAsync() => await context.Products
        .Include(p => p.Categories).Include(p => p.ProductStock)
        .AsNoTracking().OrderBy(p => p.Name).ToListAsync();

    public Task<Product?> GetByIdAsync(int id) => context.Products
        .Include(p => p.Categories).Include(p => p.ProductStock)
        .FirstOrDefaultAsync(p => p.Id == id);

    public Task<bool> ExistsByNameAsync(string name, int? excludeId = null) => context.Products
        .AnyAsync(p => p.Name == name && (!excludeId.HasValue || p.Id != excludeId.Value));

    public Task<bool> ExistsByBarcodeAsync(string barcode, int? excludeId = null) => context.Products
        .AnyAsync(p => p.Barcode == barcode && (!excludeId.HasValue || p.Id != excludeId.Value));

    public async Task CreateAsync(Product product)
    {
        context.Products.Add(product);
        await SaveAsync();
    }

    public Task UpdateAsync(Product product) => SaveAsync();

    public async Task DeleteAsync(int id)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();
        var stock = await context.ProductStocks
            .FromSqlInterpolated($"SELECT * FROM dbo.ProductStocks WITH (UPDLOCK, HOLDLOCK) WHERE ProductId = {id}")
            .SingleOrDefaultAsync();
        var product = await context.Products.FindAsync(id)
            ?? throw new InvalidOperationException("El producto ya no existe.");
        if (stock is null)
            throw new InvalidOperationException("El producto no tiene un registro de stock válido.");
        if (await context.StockMovements.AnyAsync(m => m.ProductId == id))
            throw new InvalidOperationException("No se puede eliminar un producto con historial de movimientos de stock.");
        if (stock.Quantity != 0)
            throw new InvalidOperationException("No se puede eliminar un producto con existencias positivas o negativas.");
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
