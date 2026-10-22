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

    public async Task<IEnumerable<Categoria>> GetAllAsync()
    {
        return await _context.Categorias
            .Where(c => c.Activo)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Categoria?> GetByIdAsync(int idCategoria)
    {
        return await _context.Categorias
            .FirstOrDefaultAsync(c => c.IdCategoria == idCategoria && c.Activo);
    }

    public async Task<bool> ExistsByNameAsync(string nombre, int? excludeId = null)
    {
        return await _context.Categorias
            .AnyAsync(c => c.Activo && c.Nombre == nombre && (!excludeId.HasValue || c.IdCategoria != excludeId.Value));
    }

    public async Task CreateAsync(Categoria categoria)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            await _context.Categorias.AddAsync(categoria);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task UpdateAsync(Categoria categoria)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var tracked = await _context.Categorias.FindAsync(categoria.IdCategoria);
            if (tracked is null)
                throw new InvalidOperationException($"No se encontró la categoría con id {categoria.IdCategoria}.");

            tracked.Nombre = categoria.Nombre;
            tracked.Descripcion = categoria.Descripcion;
            tracked.Activo = categoria.Activo;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
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
            var tracked = await _context.Categorias.FindAsync(idCategoria);
            if (tracked is null) return;

            tracked.Activo = false;
            _context.Entry(tracked).Property(c => c.Activo).IsModified = true;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}