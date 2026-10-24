using VentaStock.Domain.Entities;

namespace VentaStock.Domain.Contracts;

public interface ICustomerRepository
{
    Task<IReadOnlyList<Customer>> GetAllAsync(string? search);
    Task<Customer?> GetByIdAsync(int id);
    Task<bool> ExistsByDocumentAsync(string documentNumber, int? excludeId = null);
    Task CreateAsync(Customer customer);
    Task UpdateAsync(Customer customer, byte[] version);
}
