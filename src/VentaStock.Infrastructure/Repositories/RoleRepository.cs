using Microsoft.EntityFrameworkCore;
using VentaStock.Domain.Contracts;
using VentaStock.Domain.Entities;
using VentaStock.Infrastructure.Data;

namespace VentaStock.Infrastructure.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly VentaStockDbContext _context;

    public RoleRepository(VentaStockDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Rol>> GetAllAsync()
    {
        return await _context.Rols
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Rol?> GetByIdAsync(int roleId)
    {
        return await _context.Rols
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.IdRol == roleId);
    }
}