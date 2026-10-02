using AutoMapper;
using VentaStock.Application.DTOs;
using VentaStock.Domain.Contracts;
using VentaStock.Domain.Entities;

namespace VentaStock.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;

    public CategoryService(ICategoryRepository categoryRepository, IMapper mapper)
    {
        _categoryRepository = categoryRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<CategoryDto>> GetAllAsync()
    {
        var categories = await _categoryRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<CategoryDto>>(categories);
    }

    public async Task<CategoryDto?> GetByIdAsync(int idCategoria)
    {
        var category = await _categoryRepository.GetByIdAsync(idCategoria);
        return category is null ? null : _mapper.Map<CategoryDto>(category);
    }

    public async Task<EditCategoryDto?> GetForEditByIdAsync(int idCategoria)
    {
        var category = await _categoryRepository.GetByIdAsync(idCategoria);
        return category is null ? null : _mapper.Map<EditCategoryDto>(category);
    }

    public async Task CreateAsync(CreateCategoryDto dto)
    {
        var exists = await _categoryRepository.ExistsByNameAsync(dto.Nombre.Trim());
        if (exists)
            throw new InvalidOperationException($"Ya existe una categoría activa con el nombre '{dto.Nombre}'.");

        var category = _mapper.Map<Categoria>(dto);
        category.Nombre = dto.Nombre.Trim();
        category.Activo = true;

        await _categoryRepository.CreateAsync(category);
    }

    public async Task UpdateAsync(EditCategoryDto dto)
    {
        var category = await _categoryRepository.GetByIdAsync(dto.IdCategoria);
        if (category is null)
            throw new InvalidOperationException($"No se encontró la categoría con id {dto.IdCategoria}.");

        var exists = await _categoryRepository.ExistsByNameAsync(dto.Nombre.Trim(), dto.IdCategoria);
        if (exists)
            throw new InvalidOperationException($"Ya existe otra categoría activa con el nombre '{dto.Nombre}'.");

        category.Nombre = dto.Nombre.Trim();
        category.Descripcion = dto.Descripcion;

        await _categoryRepository.UpdateAsync(category);
    }

    public async Task DeleteAsync(int idCategoria)
    {
        await _categoryRepository.DeleteAsync(idCategoria);
    }

    public async Task<DeleteCategoryDto?> GetForDeleteByIdAsync(int idCategoria)
    {
        var category = await _categoryRepository.GetByIdAsync(idCategoria);
        return category is null ? null : _mapper.Map<DeleteCategoryDto>(category);
    }
}