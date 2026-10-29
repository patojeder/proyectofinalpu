using VentaStock.Domain.Entities;

namespace VentaStock.Domain.Contracts;

public interface IBranchRepository
{
    Task<IEnumerable<Branch>> GetAllAsync(string? searchTerm = null);
    Task<Branch?> GetByIdAsync(int id);
    Task<bool> ExistsByNameAsync(string name, int? excludeId = null);
    Task CreateAsync(Branch branch);
    Task UpdateAsync(Branch branch);
    Task DeleteAsync(int id);
}

