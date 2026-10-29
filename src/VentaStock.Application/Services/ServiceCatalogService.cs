using AutoMapper;
using VentaStock.Application.Contracts;
using VentaStock.Application.DTOs;
using VentaStock.Domain.Contracts;
using VentaStock.Domain.Entities;
using VentaStock.Domain.Security.Permissions;

namespace VentaStock.Application.Services;

public class ServiceCatalogService(
    IServiceRepository serviceRepository,
    ICategoryRepository categoryRepository,
    IMapper mapper,
    ICurrentUser currentUser) : IServiceCatalogService
{
    public async Task<IEnumerable<ServiceDto>> GetAllAsync(string? searchTerm = null)
    {
        RequirePermission(ServicePermissions.Read);
        return mapper.Map<IEnumerable<ServiceDto>>(await serviceRepository.GetAllAsync(searchTerm));
    }

    public async Task<EditServiceDto?> GetForEditByIdAsync(int id)
    {
        RequirePermission(ServicePermissions.Update);
        var service = await serviceRepository.GetByIdAsync(id);
        return service is null ? null : mapper.Map<EditServiceDto>(service);
    }

    public async Task<DeleteServiceDto?> GetForDeleteByIdAsync(int id)
    {
        RequirePermission(ServicePermissions.Delete);
        var service = await serviceRepository.GetByIdAsync(id);
        return service is null ? null : mapper.Map<DeleteServiceDto>(service);
    }

    public async Task<IEnumerable<CategorySelectDto>> GetCategoriesForSelectAsync()
    {
        if (!currentUser.HasPermission(ServicePermissions.Create) && !currentUser.HasPermission(ServicePermissions.Update))
            throw new UnauthorizedAccessException();

        var categories = await categoryRepository.GetAllAsync();
        return categories.OrderBy(c => c.Name).Select(c => new CategorySelectDto { Id = c.Id, Name = c.Name });
    }

    public async Task CreateAsync(CreateServiceDto dto)
    {
        RequirePermission(ServicePermissions.Create);
        var name = ValidateName(dto.Name);
        if (await serviceRepository.ExistsByNameAsync(name))
            throw new InvalidOperationException("Ya existe un servicio con ese nombre.");

        var categories = await GetSelectedCategoriesAsync(dto.CategoryIds);
        await serviceRepository.CreateAsync(new Service
        {
            Name = name,
            Description = dto.Description?.Trim(),
            Categories = categories
        });
    }

    public async Task UpdateAsync(EditServiceDto dto)
    {
        RequirePermission(ServicePermissions.Update);
        var service = await serviceRepository.GetByIdAsync(dto.Id)
            ?? throw new InvalidOperationException("El servicio ya no existe.");
        var name = ValidateName(dto.Name);
        if (await serviceRepository.ExistsByNameAsync(name, dto.Id))
            throw new InvalidOperationException("Ya existe otro servicio con ese nombre.");

        var categories = await GetSelectedCategoriesAsync(dto.CategoryIds);
        service.Name = name;
        service.Description = dto.Description?.Trim();
        service.Categories = categories;
        await serviceRepository.UpdateAsync(service);
    }

    public async Task DeleteAsync(int id)
    {
        RequirePermission(ServicePermissions.Delete);
        await serviceRepository.DeleteAsync(id);
    }

    private async Task<List<Category>> GetSelectedCategoriesAsync(IEnumerable<int>? selectedIds)
    {
        var ids = (selectedIds ?? []).Distinct().ToArray();
        if (ids.Any(id => id <= 0))
            throw new InvalidOperationException("La selección de categorías no es válida.");
        var categories = await categoryRepository.GetByIdsAsync(ids);
        if (categories.Count != ids.Length)
            throw new InvalidOperationException("Una de las categorías seleccionadas ya no existe.");
        return categories;
    }

    private static string ValidateName(string? name)
    {
        var trimmed = name?.Trim();
        if (string.IsNullOrEmpty(trimmed) || trimmed.Length > 100)
            throw new InvalidOperationException("El nombre es obligatorio y no puede superar los 100 caracteres.");
        return trimmed;
    }

    private void RequirePermission(string permission)
    {
        if (!currentUser.IsAuthenticated || !currentUser.HasPermission(permission))
            throw new UnauthorizedAccessException();
    }
}
