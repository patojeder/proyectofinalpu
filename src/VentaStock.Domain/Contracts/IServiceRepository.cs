using VentaStock.Domain.Entities;

namespace VentaStock.Domain.Contracts;

public interface IServiceRepository
{
    Task<IEnumerable<Service>> GetAllAsync(string? searchTerm = null);
    Task<Service?> GetByIdAsync(int id);
    Task<bool> ExistsByNameAsync(string name, int? excludeId = null);
    Task CreateAsync(Service service);
    Task UpdateAsync(Service service);
    Task DeleteAsync(int id);
}
