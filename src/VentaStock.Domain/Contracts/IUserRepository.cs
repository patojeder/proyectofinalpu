using VentaStock.Domain.Entities;

namespace VentaStock.Domain.Contracts;

public interface IUserRepository
{
    Task<Usuario?> GetByUsernameAsync(string username);
    Task<Usuario?> GetByIdAsync(int userId);
    Task<IEnumerable<Usuario>> GetAllAsync();
    Task<bool> ExistsUsernameAsync(string username, int? excludeId = null);
    Task<bool> ExistsMailAsync(string mail, int? excludeId = null);
    Task CreateAsync(Usuario user);
    Task UpdateAsync(Usuario user);
    Task DeleteAsync(int userId);
}