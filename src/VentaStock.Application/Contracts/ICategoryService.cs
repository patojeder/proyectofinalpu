using VentaStock.Application.DTOs;

namespace VentaStock.Application.Services;

public interface ICategoryService
{
    Task<IEnumerable<CategoryDto>> GetAllAsync();
    Task<CategoryDto?> GetByIdAsync(int idCategoria);
    Task<EditCategoryDto?> GetForEditByIdAsync(int idCategoria);
    Task<DeleteCategoryDto?> GetForDeleteByIdAsync(int idCategoria);
    Task CreateAsync(CreateCategoryDto dto);
    Task UpdateAsync(EditCategoryDto dto);
    Task DeleteAsync(int idCategoria);
}