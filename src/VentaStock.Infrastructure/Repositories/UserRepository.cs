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

    public async Task<User?> GetByUserNameAsync(string username)
    {
        return await _context.Users
            .Include(u => u.Role)
                .ThenInclude(r => r.Permissions)
            .FirstOrDefaultAsync(u => u.UserName == username);
    }

    public async Task<User?> GetByIdAsync(int userId)
    {
        return await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _context.Users
            .Include(u => u.Role)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<bool> ExistsUsernameAsync(string username, int? excludeId = null)
    {
        return await _context.Users
            .AnyAsync(u => u.UserName == username && (!excludeId.HasValue || u.Id != excludeId.Value));
    }

    public async Task CreateAsync(User user)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task UpdateAsync(User user)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var ownerIds = await LockOwnersAsync();
            var trackedUser = await _context.Users.FindAsync(user.Id);
            if (trackedUser is null)
                throw new InvalidOperationException($"No se encontro al usuario con id {user.Id}.");

            // Recargar después del bloqueo: FindAsync puede devolver datos desactualizados.
            await _context.Entry(trackedUser).ReloadAsync();
            if (_context.Entry(trackedUser).State == EntityState.Detached)
                throw new InvalidOperationException("El usuario ya no existe.");

            var newRole = await _context.Roles.AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == user.RoleId)
                ?? throw new InvalidOperationException("El rol seleccionado no existe.");

            bool remainsOwner = string.Equals(newRole.Name, SystemRoles.Owner, StringComparison.OrdinalIgnoreCase);
            if (ownerIds.Count == 1 && ownerIds.Contains(user.Id) && !remainsOwner)
                throw new InvalidOperationException(LastOwnerMessage);

            trackedUser.UserName = user.UserName;
            trackedUser.RoleId = user.RoleId;

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
            var user = await _context.Users.FindAsync(userId);
            if (user is null) return;

            await _context.Entry(user).ReloadAsync();
            if (_context.Entry(user).State == EntityState.Detached) return;

            if (ownerIds.Count == 1 && ownerIds.Contains(userId))
                throw new InvalidOperationException(LastOwnerMessage);

            _context.Users.Remove(user);

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
            SELECT u.Id AS [Value]
            FROM dbo.Users AS u WITH (UPDLOCK, HOLDLOCK)
            INNER JOIN dbo.Roles AS r ON r.Id = u.RoleId
            WHERE r.Name = {SystemRoles.Owner}
            """).ToListAsync();
    }
}
