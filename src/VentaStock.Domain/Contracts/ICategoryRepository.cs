using VentaStock.Domain.Entities;

namespace VentaStock.Domain.Contracts;

public interface ICategoryRepository
{
    Task<IEnumerable<Category>> GetAllAsync();
    Task<List<Category>> GetByIdsAsync(IEnumerable<int> ids);
    Task<Category?> GetByIdAsync(int idCategoria);
    Task<bool> ExistsByNameAsync(string nombre, int? excludeId = null);
    Task CreateAsync(Category categoria);
    Task UpdateAsync(Category categoria);
    Task DeleteAsync(int idCategoria);
}
