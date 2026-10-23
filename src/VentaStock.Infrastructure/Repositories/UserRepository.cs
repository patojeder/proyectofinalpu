using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using VentaStock.Domain.Contracts;
using VentaStock.Domain.Entities;
using VentaStock.Domain.Security;
using VentaStock.Infrastructure.Data;

namespace VentaStock.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private const string LastOwnerMessage = "Debe permanecer al menos un Dueño en el sistema.";
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
            .FirstOrDefaultAsync(u => u.NombreUsuario == username);
    }

    public async Task<Usuario?> GetByIdAsync(int userId)
    {
        return await _context.Usuarios
            .Include(u => u.IdRolNavigation)
            .FirstOrDefaultAsync(u => u.IdUsuario == userId);
    }

    public async Task<IEnumerable<Usuario>> GetAllAsync()
    {
        return await _context.Usuarios
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
            var ownerIds = await LockOwnersAsync();
            var trackedUser = await _context.Usuarios.FindAsync(user.IdUsuario);
            if (trackedUser is null)
                throw new InvalidOperationException($"No se encontro al usuario con id {user.IdUsuario}.");

            // Recargar después del bloqueo: FindAsync puede devolver datos desactualizados.
            await _context.Entry(trackedUser).ReloadAsync();
            if (_context.Entry(trackedUser).State == EntityState.Detached)
                throw new InvalidOperationException("El usuario ya no existe.");

            var newRole = await _context.Rols.AsNoTracking()
                .FirstOrDefaultAsync(r => r.IdRol == user.IdRol)
                ?? throw new InvalidOperationException("El rol seleccionado no existe.");

            bool remainsOwner = string.Equals(newRole.Nombre, SystemRoles.Owner, StringComparison.OrdinalIgnoreCase);
            if (ownerIds.Count == 1 && ownerIds.Contains(user.IdUsuario) && !remainsOwner)
                throw new InvalidOperationException(LastOwnerMessage);

            trackedUser.NombreUsuario = user.NombreUsuario;
            trackedUser.Mail = user.Mail;
            trackedUser.IdRol = user.IdRol;

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
            var ownerIds = await LockOwnersAsync();
            var user = await _context.Usuarios.FindAsync(userId);
            if (user is null) return;

            await _context.Entry(user).ReloadAsync();
            if (_context.Entry(user).State == EntityState.Detached) return;

            if (ownerIds.Count == 1 && ownerIds.Contains(userId))
                throw new InvalidOperationException(LastOwnerMessage);

            _context.Usuarios.Remove(user);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 547 })
        {
            await transaction.RollbackAsync();
            throw new InvalidOperationException("No se puede eliminar el usuario porque tiene registros relacionados.", ex);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private Task<List<int>> LockOwnersAsync()
    {
        // Mantener los bloqueos hasta confirmar la transacción para proteger al último Dueño.
        return _context.Database.SqlQuery<int>($"""
            SELECT u.id_usuario AS [Value]
            FROM dbo.usuario AS u WITH (UPDLOCK, HOLDLOCK)
            INNER JOIN dbo.rol AS r ON r.id_rol = u.id_rol
            WHERE r.nombre = {SystemRoles.Owner}
            """).ToListAsync();
    }
}
