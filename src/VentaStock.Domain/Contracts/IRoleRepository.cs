using VentaStock.Domain.Entities;

namespace VentaStock.Domain.Contracts;

public interface IRoleRepository
{
    Task<IEnumerable<Role>> GetAllAsync();
    Task<Role?> GetByIdAsync(int roleId);
}