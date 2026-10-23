using VentaStock.Application.Contracts;
using VentaStock.Application.DTOs;
using VentaStock.Domain.Contracts;
using VentaStock.Domain.Entities;
using VentaStock.Domain.Security.Permissions;

namespace VentaStock.Application.Services;

public class ProductService(
    IProductRepository productRepository,
    ICategoryRepository categoryRepository,
    ICurrentUser currentUser) : IProductService
{
    public async Task<IEnumerable<ProductDto>> GetAllAsync()
    {
        RequirePermission(ProductPermissions.Read);
        return (await productRepository.GetAllAsync()).Select(ToDto).ToList();
    }

    public async Task<EditProductDto?> GetForEditByIdAsync(int id)
    {
        RequirePermission(ProductPermissions.Update);
        var product = await productRepository.GetByIdAsync(id);
        return product is null ? null : new EditProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Barcode = product.Barcode,
            Sector = product.Sector,
            Shelf = product.Shelf,
            CategoryIds = product.Categories.Select(c => c.Id).ToList(),
            MinimumQuantity = product.ProductStock?.MinimumQuantity ?? 0
        };
    }

    public async Task<ProductDto?> GetForDeleteByIdAsync(int id)
    {
        RequirePermission(ProductPermissions.Delete);
        var product = await productRepository.GetByIdAsync(id);
        return product is null ? null : ToDto(product);
    }

    public async Task<ProductPricesDto?> GetPricesByIdAsync(int id)
    {
        RequirePricePermissions();
        var product = await productRepository.GetByIdAsync(id);
        return product is null ? null : new ProductPricesDto
        {
            Id = product.Id, Name = product.Name, CostPrice = product.CostPrice, SalePrice = product.SalePrice
        };
    }

    public async Task<IEnumerable<CategorySelectDto>> GetCategoriesForSelectAsync()
    {
        if (!currentUser.IsAuthenticated || (!currentUser.HasPermission(ProductPermissions.Create)
            && !currentUser.HasPermission(ProductPermissions.Update)))
            throw new UnauthorizedAccessException();
        return (await categoryRepository.GetAllAsync())
            .Select(c => new CategorySelectDto { Id = c.Id, Name = c.Name });
    }

    public async Task CreateAsync(CreateProductDto dto)
    {
        RequirePermission(ProductPermissions.Create);
        if (dto.CostPrice.HasValue || dto.SalePrice.HasValue)
            RequirePricePermissions();
        ValidatePrices(dto.CostPrice, dto.SalePrice);
        var product = new Product();
        await ApplyFieldsAsync(product, dto);
        product.CostPrice = dto.CostPrice;
        product.SalePrice = dto.SalePrice;
        product.ProductStock = new ProductStock { Quantity = 0, MinimumQuantity = dto.MinimumQuantity ?? 0 };
        await productRepository.CreateAsync(product);
    }

    public async Task UpdateAsync(EditProductDto dto)
    {
        RequirePermission(ProductPermissions.Update);
        var product = await productRepository.GetByIdAsync(dto.Id)
            ?? throw new InvalidOperationException("El producto ya no existe.");
        if (product.ProductStock is null)
            throw new InvalidOperationException("El producto no tiene un registro de stock válido.");
        await ApplyFieldsAsync(product, dto);
        product.ProductStock.MinimumQuantity = dto.MinimumQuantity ?? 0;
        await productRepository.UpdateAsync(product);
    }

    public async Task UpdatePricesAsync(ProductPricesDto dto)
    {
        RequirePricePermissions();
        ValidatePrices(dto.CostPrice, dto.SalePrice);
        var product = await productRepository.GetByIdAsync(dto.Id)
            ?? throw new InvalidOperationException("El producto ya no existe.");
        product.CostPrice = dto.CostPrice;
        product.SalePrice = dto.SalePrice;
        await productRepository.UpdateAsync(product);
    }

    public async Task DeleteAsync(int id)
    {
        RequirePermission(ProductPermissions.Delete);
        await productRepository.DeleteAsync(id);
    }

    private async Task ApplyFieldsAsync(Product product, ProductFieldsDto dto)
    {
        var name = dto.Name?.Trim();
        if (string.IsNullOrEmpty(name) || name.Length > 150)
            throw new InvalidOperationException("El nombre es obligatorio y no puede superar los 150 caracteres.");
        var barcode = string.IsNullOrWhiteSpace(dto.Barcode)
            ? (product.Id == 0 ? "INT-" + Guid.NewGuid().ToString("N").ToUpperInvariant() : product.Barcode)
            : dto.Barcode.Trim();
        if (barcode.Length > 100 || dto.Sector?.Trim().Length > 50 || dto.Shelf?.Trim().Length > 50)
            throw new InvalidOperationException("Revisá la longitud del código (100), sector y estante (50 caracteres).");
        if (dto.MinimumQuantity < 0)
            throw new InvalidOperationException("El stock mínimo debe ser mayor o igual a 0.");
        if (await productRepository.ExistsByNameAsync(name, product.Id))
            throw new InvalidOperationException("Ya existe otro producto con ese nombre.");
        if (await productRepository.ExistsByBarcodeAsync(barcode, product.Id))
            throw new InvalidOperationException("Ya existe otro producto con ese código de barras.");
        var ids = (dto.CategoryIds ?? []).Distinct().ToArray();
        if (ids.Length == 0 || ids.Any(id => id <= 0))
            throw new InvalidOperationException("Seleccioná al menos una categoría válida.");
        var categories = await categoryRepository.GetByIdsAsync(ids);
        if (categories.Count != ids.Length)
            throw new InvalidOperationException("Una categoría seleccionada ya no existe.");
        product.Name = name;
        product.Description = dto.Description?.Trim();
        product.Barcode = barcode;
        product.Sector = dto.Sector?.Trim();
        product.Shelf = dto.Shelf?.Trim();
        product.Categories = categories;
    }

    private ProductDto ToDto(Product product) => new()
    {
        Id = product.Id,
        Name = product.Name,
        Description = product.Description,
        Barcode = product.Barcode,
        Sector = product.Sector,
        Shelf = product.Shelf,
        Categories = product.Categories.OrderBy(c => c.Name).Select(c => c.Name).ToList(),
        CostPrice = currentUser.HasPermission(ProductPermissions.ReadCost) ? product.CostPrice : null,
        SalePrice = currentUser.HasPermission(ProductPermissions.ReadSalePrice) ? product.SalePrice : null,
        Quantity = currentUser.HasPermission(StockPermissions.Read) ? product.ProductStock?.Quantity : null,
        MinimumQuantity = currentUser.HasPermission(StockPermissions.Read) ? product.ProductStock?.MinimumQuantity : null
    };

    private static void ValidatePrices(decimal? cost, decimal? sale)
    {
        if (!cost.HasValue && !sale.HasValue) return;
        if (!cost.HasValue || !sale.HasValue)
            throw new InvalidOperationException("Indicá costo y precio de venta juntos, o dejá ambos vacíos.");
        if (cost < 0 || sale <= cost)
            throw new InvalidOperationException("El costo debe ser mayor o igual a 0 y el precio de venta debe superar el costo.");
        if (cost > 9999999999999999.99m || sale > 9999999999999999.99m
            || decimal.Round(cost.Value, 2) != cost || decimal.Round(sale.Value, 2) != sale)
            throw new InvalidOperationException("Los precios admiten hasta 16 dígitos enteros y 2 decimales.");
    }

    private void RequirePricePermissions()
    {
        RequirePermission(ProductPermissions.UpdatePrices);
        RequirePermission(ProductPermissions.ReadCost);
        RequirePermission(ProductPermissions.ReadSalePrice);
    }

    private void RequirePermission(string permission)
    {
        if (!currentUser.IsAuthenticated || !currentUser.HasPermission(permission))
            throw new UnauthorizedAccessException();
    }
}
