using Microsoft.EntityFrameworkCore;
using VentaStock.Domain.Contracts;
using VentaStock.Domain.Entities;
using VentaStock.Infrastructure.Data;

namespace VentaStock.Infrastructure.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly VentaStockDbContext _context;

    public UsuarioRepository(VentaStockDbContext context)
    {
        _context = context;
    }

    public async Task<Usuario?> ObtenerPorNombreUsuarioAsync(string nombreUsuario, CancellationToken cancellationToken = default)
    {
        return await _context.Usuarios
            .Include(u => u.IdRolNavigation)
                .ThenInclude(r => r.IdPermisos)
            .FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario, cancellationToken);
    }

    public async Task<Usuario?> ObtenerPorIdAsync(int idUsuario, CancellationToken cancellationToken = default)
    {
        return await _context.Usuarios
            .Include(u => u.IdRolNavigation)
            .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario, cancellationToken);
    }

    public async Task GuardarCambiosAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}