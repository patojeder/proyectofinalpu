using VentaStock.Application.DTOs;

namespace VentaStock.Application.Contracts;

public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetAllAsync();
    Task<EditProductDto?> GetForEditByIdAsync(int id);
    Task<ProductDto?> GetForDeleteByIdAsync(int id);
    Task<ProductPricesDto?> GetPricesByIdAsync(int id);
    Task<IEnumerable<CategorySelectDto>> GetCategoriesForSelectAsync();
    Task CreateAsync(CreateProductDto dto);
    Task UpdateAsync(EditProductDto dto);
    Task UpdatePricesAsync(ProductPricesDto dto);
    Task DeleteAsync(int id);
}
