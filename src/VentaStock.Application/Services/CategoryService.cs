using AutoMapper;
using VentaStock.Application.Contracts;
using VentaStock.Application.DTOs;
using VentaStock.Domain.Security.Permissions;
using VentaStock.Domain.Contracts;
using VentaStock.Domain.Entities;

namespace VentaStock.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;
    private readonly ICurrentUser _currentUser;

    public CategoryService(ICategoryRepository categoryRepository, IMapper mapper, ICurrentUser currentUser)
    {
        _categoryRepository = categoryRepository;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<CategoryDto>> GetAllAsync()
    {
        RequirePermission(CategoryPermissions.Read);
        var categories = await _categoryRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<CategoryDto>>(categories);
    }

    public async Task<CategoryDto?> GetByIdAsync(int idCategoria)
    {
        RequirePermission(CategoryPermissions.Read);
        var category = await _categoryRepository.GetByIdAsync(idCategoria);
        return category is null ? null : _mapper.Map<CategoryDto>(category);
    }

    public async Task<EditCategoryDto?> GetForEditByIdAsync(int idCategoria)
    {
        RequirePermission(CategoryPermissions.Update);
        var category = await _categoryRepository.GetByIdAsync(idCategoria);
        return category is null ? null : _mapper.Map<EditCategoryDto>(category);
    }

    public async Task CreateAsync(CreateCategoryDto dto)
    {
        RequirePermission(CategoryPermissions.Create);
        var exists = await _categoryRepository.ExistsByNameAsync(dto.Name.Trim());
        if (exists)
            throw new InvalidOperationException($"Ya existe una categoría con el nombre '{dto.Name}'.");

        var category = _mapper.Map<Category>(dto);
        category.Name = dto.Name.Trim();

        await _categoryRepository.CreateAsync(category);
    }

    public async Task UpdateAsync(EditCategoryDto dto)
    {
        RequirePermission(CategoryPermissions.Update);
        var category = await _categoryRepository.GetByIdAsync(dto.Id);
        if (category is null)
            throw new InvalidOperationException($"No se encontró la categoría con id {dto.Id}.");

        var exists = await _categoryRepository.ExistsByNameAsync(dto.Name.Trim(), dto.Id);
        if (exists)
            throw new InvalidOperationException($"Ya existe otra categoría con el nombre '{dto.Name}'.");

        category.Name = dto.Name.Trim();
        category.Description = dto.Description;

        await _categoryRepository.UpdateAsync(category);
    }

    public async Task DeleteAsync(int idCategoria)
    {
        RequirePermission(CategoryPermissions.Delete);
        await _categoryRepository.DeleteAsync(idCategoria);
    }

    public async Task<DeleteCategoryDto?> GetForDeleteByIdAsync(int idCategoria)
    {
        RequirePermission(CategoryPermissions.Delete);
        var category = await _categoryRepository.GetByIdAsync(idCategoria);
        return category is null ? null : _mapper.Map<DeleteCategoryDto>(category);
    }

    private void RequirePermission(string permission)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.HasPermission(permission))
            throw new UnauthorizedAccessException();
    }
}
