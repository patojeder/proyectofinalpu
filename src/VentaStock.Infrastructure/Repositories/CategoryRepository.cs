using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using VentaStock.Domain.Contracts;
using VentaStock.Domain.Entities;
using VentaStock.Infrastructure.Data;

namespace VentaStock.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly VentaStockDbContext _context;

    public CategoryRepository(VentaStockDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Category>> GetAllAsync(string? searchTerm = null)
    {
        var query = _context.Categories.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim();
            query = query.Where(c => c.Name.Contains(term)
                || (c.Description != null && c.Description.Contains(term)));
        }

        return await query.OrderBy(c => c.Name).ToListAsync();
    }

    public Task<List<Category>> GetByIdsAsync(IEnumerable<int> ids) => _context.Categories
        .Where(c => ids.Contains(c.Id))
        .ToListAsync();

    public async Task<Category?> GetByIdAsync(int idCategoria)
    {
        return await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == idCategoria);
    }

    public async Task<bool> ExistsByNameAsync(string nombre, int? excludeId = null)
    {
        return await _context.Categories
            .AnyAsync(c => c.Name == nombre && (!excludeId.HasValue || c.Id != excludeId.Value));
    }

    public async Task CreateAsync(Category categoria)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            await _context.Categories.AddAsync(categoria);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            await transaction.RollbackAsync();
            throw new InvalidOperationException("Ya existe una categoría con ese nombre.", ex);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task UpdateAsync(Category categoria)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var tracked = await _context.Categories.FindAsync(categoria.Id);
            if (tracked is null)
                throw new InvalidOperationException($"No se encontró la categoría con id {categoria.Id}.");

            tracked.Name = categoria.Name;
            tracked.Description = categoria.Description;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            await transaction.RollbackAsync();
            throw new InvalidOperationException("Ya existe otra categoría con ese nombre.", ex);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task DeleteAsync(int idCategoria)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var tracked = await _context.Categories.FindAsync(idCategoria);
            if (tracked is null) return;

            _context.Categories.Remove(tracked);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 547 })
        {
            await transaction.RollbackAsync();
            throw new InvalidOperationException("No se puede eliminar la categoría porque tiene registros relacionados.", ex);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
