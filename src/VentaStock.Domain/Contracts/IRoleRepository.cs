using VentaStock.Domain.Entities;

namespace VentaStock.Domain.Contracts;

public interface IRoleRepository
{
    Task<IEnumerable<Rol>> GetAllAsync();
    Task<Rol?> GetByIdAsync(int roleId);
}