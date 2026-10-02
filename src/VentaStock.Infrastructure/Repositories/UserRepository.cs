using Microsoft.EntityFrameworkCore;
using VentaStock.Domain.Contracts;
using VentaStock.Domain.Entities;
using VentaStock.Infrastructure.Data;

namespace VentaStock.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly VentaStockDbContext _context;

    public UserRepository(VentaStockDbContext context)
    {
        _context = context;
    }

    public async Task<Usuario?> GetByUsernameAsync(string username)
    {
        return await _context.Usuarios
            .Include(u => u.IdRolNavigation)
                .ThenInclude(r => r.IdPermisos)
            .FirstOrDefaultAsync(u => u.NombreUsuario == username && u.Activo);
    }

    public async Task<Usuario?> GetByIdAsync(int userId)
    {
        return await _context.Usuarios
            .Include(u => u.IdRolNavigation)
            .FirstOrDefaultAsync(u => u.IdUsuario == userId && u.Activo);
    }

    public async Task<IEnumerable<Usuario>> GetAllAsync()
    {
        return await _context.Usuarios
            .Where(u => u.Activo) // <-- Este filtro excluye los dados de baja
            .Include(u => u.IdRolNavigation)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<bool> ExistsUsernameAsync(string username, int? excludeId = null)
    {
        return await _context.Usuarios
            .AnyAsync(u => u.NombreUsuario == username && (!excludeId.HasValue || u.IdUsuario != excludeId.Value));
    }

    public async Task<bool> ExistsMailAsync(string mail, int? excludeId = null)
    {
        return await _context.Usuarios
            .AnyAsync(u => u.Mail == mail && (!excludeId.HasValue || u.IdUsuario != excludeId.Value));
    }

    public async Task CreateAsync(Usuario user)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            await _context.Usuarios.AddAsync(user);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task UpdateAsync(Usuario user)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var trackedUser = await _context.Usuarios.FindAsync(user.IdUsuario);
            if (trackedUser is null)
                throw new InvalidOperationException($"No se encontro al usuario con id {user.IdUsuario}.");

            trackedUser.NombreUsuario = user.NombreUsuario;
            trackedUser.Mail = user.Mail;
            trackedUser.IdRol = user.IdRol;
            trackedUser.Activo = user.Activo;

            if (!string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                trackedUser.PasswordHash = user.PasswordHash;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task DeleteAsync(int userId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var user = await _context.Usuarios.FindAsync(userId);
            if (user is null) return;

            user.Activo = false;
            _context.Entry(user).Property(u => u.Activo).IsModified = true;

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