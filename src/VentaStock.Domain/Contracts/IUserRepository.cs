using VentaStock.Domain.Entities;

namespace VentaStock.Domain.Contracts;

public interface IUserRepository
{
    Task<User?> GetByUserNameAsync(string username);
    Task<User?> GetByIdAsync(int userId);
    Task<IEnumerable<User>> GetAllAsync();
    Task<bool> ExistsUsernameAsync(string username, int? excludeId = null);
    Task<bool> ExistsEmailAsync(string mail, int? excludeId = null);
    Task CreateAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(int userId);
}