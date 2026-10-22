using VentaStock.Domain.Entities;

namespace VentaStock.Domain.Contracts;

public interface ICategoryRepository
{
    Task<IEnumerable<Categoria>> GetAllAsync();
    Task<Categoria?> GetByIdAsync(int idCategoria);
    Task<bool> ExistsByNameAsync(string nombre, int? excludeId = null);
    Task CreateAsync(Categoria categoria);
    Task UpdateAsync(Categoria categoria);
    Task DeleteAsync(int idCategoria);
}