using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using VentaStock.Domain.Contracts;
using VentaStock.Domain.Entities;
using VentaStock.Infrastructure.Data;

namespace VentaStock.Infrastructure.Repositories;

public class CustomerRepository(VentaStockDbContext context) : ICustomerRepository
{
    public async Task<IReadOnlyList<Customer>> GetAllAsync(string? search)
    {
        var query = context.Customers.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var digits = new string(search.Where(char.IsAsciiDigit).ToArray());
            var numericSearch = digits.Length > 0 && search.All(c => char.IsAsciiDigit(c) || " .-()+".Contains(c));
            query = query.Where(c => (c.FirstName + " " + c.LastName).Contains(search)
                || (c.LastName + " " + c.FirstName).Contains(search)
                || (numericSearch && (c.DocumentNumber.Contains(digits)
                    || (c.Phone != null && c.Phone.Replace(" ", "").Replace("-", "").Replace("(", "")
                        .Replace(")", "").Replace(".", "").Replace("+", "").Contains(digits)))));
        }
        return await query.OrderBy(c => c.LastName).ThenBy(c => c.FirstName).ToListAsync();
    }

    public Task<Customer?> GetByIdAsync(int id) => context.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);

    public Task<bool> ExistsByDocumentAsync(string documentNumber, int? excludeId = null) => context.Customers
        .AnyAsync(c => c.DocumentNumber == documentNumber && (!excludeId.HasValue || c.Id != excludeId.Value));

    public async Task CreateAsync(Customer customer)
    {
        context.Customers.Add(customer);
        await SaveAsync();
    }

    public async Task UpdateAsync(Customer customer, byte[] version)
    {
        context.Customers.Update(customer);
        context.Entry(customer).Property(c => c.Version).OriginalValue = version;
        await SaveAsync();
    }

    private async Task SaveAsync()
    {
        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new InvalidOperationException("Otro usuario modificó el cliente. Recargá la pantalla y revisá sus datos antes de guardar.", ex);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            throw new InvalidOperationException("Ya existe un cliente con ese documento.", ex);
        }
    }
}
