using VentaStock.Domain.Entities;

namespace VentaStock.Domain.Contracts;

public interface IUserRepository
{
    Task<User?> GetByUserNameAsync(string username);
    Task<User?> GetSessionByIdAsync(int userId);
    Task<User?> GetByIdAsync(int userId);
    Task<IEnumerable<User>> GetAllAsync();
    Task<bool> ExistsUsernameAsync(string username, int? excludeId = null);
    Task CreateAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(int userId);
}
