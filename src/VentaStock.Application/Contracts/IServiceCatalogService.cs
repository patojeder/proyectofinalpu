using VentaStock.Application.DTOs;

namespace VentaStock.Application.Contracts;

public interface IServiceCatalogService
{
    Task<IEnumerable<ServiceDto>> GetAllAsync();
    Task<EditServiceDto?> GetForEditByIdAsync(int id);
    Task<DeleteServiceDto?> GetForDeleteByIdAsync(int id);
    Task<IEnumerable<CategorySelectDto>> GetCategoriesForSelectAsync();
    Task CreateAsync(CreateServiceDto dto);
    Task UpdateAsync(EditServiceDto dto);
    Task DeleteAsync(int id);
}
