using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using VentaStock.Domain.Contracts;
using VentaStock.Domain.Entities;
using VentaStock.Infrastructure.Data;

namespace VentaStock.Infrastructure.Repositories;

public class ServiceRepository(VentaStockDbContext context) : IServiceRepository
{
    public async Task<IEnumerable<Service>> GetAllAsync() => await context.Services
        .Include(s => s.Categories)
        .AsNoTracking()
        .OrderBy(s => s.Name)
        .ToListAsync();

    public Task<Service?> GetByIdAsync(int id) => context.Services
        .Include(s => s.Categories)
        .FirstOrDefaultAsync(s => s.Id == id);

    public Task<bool> ExistsByNameAsync(string name, int? excludeId = null) => context.Services
        .AnyAsync(s => s.Name == name && (!excludeId.HasValue || s.Id != excludeId.Value));

    public async Task CreateAsync(Service service)
    {
        context.Services.Add(service);
        await SaveAsync();
    }

    public async Task UpdateAsync(Service service)
    {
        context.Entry(service).State = EntityState.Modified;
        await SaveAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var service = await context.Services.FindAsync(id)
            ?? throw new InvalidOperationException("El servicio ya no existe.");
        context.Services.Remove(service);
        await SaveAsync();
    }

    private async Task SaveAsync()
    {
        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            throw new InvalidOperationException("Ya existe un servicio con ese nombre.", ex);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 547 })
        {
            throw new InvalidOperationException("La operación no pudo completarse porque existen registros relacionados o una categoría ya no está disponible.", ex);
        }
    }
}
