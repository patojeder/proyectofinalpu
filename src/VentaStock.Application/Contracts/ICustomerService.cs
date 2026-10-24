using VentaStock.Application.DTOs;

namespace VentaStock.Application.Contracts;

public interface ICustomerService
{
    Task<CustomerIndexDto> GetAllAsync(string? search);
    Task<EditCustomerDto?> GetForEditAsync(int id);
    Task CreateAsync(CreateCustomerDto dto);
    Task UpdateAsync(EditCustomerDto dto);
}
