using VentaStock.Domain.Entities;

namespace VentaStock.Domain.Contracts;

public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAllAsync(string? searchTerm = null);
    Task<Product?> GetByIdAsync(int id);
    Task<bool> ExistsByNameAsync(string name, int? excludeId = null);
    Task<bool> ExistsByBarcodeAsync(string barcode, int? excludeId = null);
    Task CreateAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(int id);
}
