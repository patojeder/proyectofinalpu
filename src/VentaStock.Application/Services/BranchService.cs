using VentaStock.Application.Contracts;
using VentaStock.Application.DTOs;
using VentaStock.Domain.Contracts;
using VentaStock.Domain.Entities;

namespace VentaStock.Application.Services;

public class BranchService(IBranchRepository branchRepository, ICurrentUser currentUser) : IBranchService
{
    public async Task<IEnumerable<BranchDto>> GetAllAsync(string? searchTerm = null)
    {
        RequireOwner();
        return (await branchRepository.GetAllAsync(searchTerm)).Select(ToDto).ToList();
    }

    public async Task<EditBranchDto?> GetForEditByIdAsync(int id)
    {
        RequireOwner();
        var branch = await branchRepository.GetByIdAsync(id);
        return branch is null ? null : new EditBranchDto
        {
            Id = branch.Id,
            Name = branch.Name,
            Address = branch.Address,
            Phone = branch.Phone
        };
    }

    public async Task<BranchDto?> GetForDeleteByIdAsync(int id)
    {
        RequireOwner();
        var branch = await branchRepository.GetByIdAsync(id);
        return branch is null ? null : ToDto(branch);
    }

    public async Task CreateAsync(CreateBranchDto dto)
    {
        RequireOwner();
        var branch = new Branch();
        await ApplyFieldsAsync(branch, dto);
        await branchRepository.CreateAsync(branch);
    }

    public async Task UpdateAsync(EditBranchDto dto)
    {
        RequireOwner();
        var branch = await branchRepository.GetByIdAsync(dto.Id)
            ?? throw new InvalidOperationException("La sucursal ya no existe.");
        await ApplyFieldsAsync(branch, dto);
        await branchRepository.UpdateAsync(branch);
    }

    public async Task DeleteAsync(int id)
    {
        RequireOwner();
        await branchRepository.DeleteAsync(id);
    }

    private async Task ApplyFieldsAsync(Branch branch, BranchFieldsDto dto)
    {
        var name = dto.Name?.Trim();
        if (string.IsNullOrEmpty(name) || name.Length > 100)
            throw new InvalidOperationException("El nombre es obligatorio y no puede superar los 100 caracteres.");

        var address = string.IsNullOrWhiteSpace(dto.Address) ? null : dto.Address.Trim();
        var phone = string.IsNullOrWhiteSpace(dto.Phone) ? null : dto.Phone.Trim();

        if (address?.Length > 250 || phone?.Length > 50)
            throw new InvalidOperationException("Revisá la longitud de la dirección (250) y teléfono (50 caracteres).");

        if (await branchRepository.ExistsByNameAsync(name, branch.Id == 0 ? null : branch.Id))
            throw new InvalidOperationException("Ya existe otra sucursal con ese nombre.");

        branch.Name = name;
        branch.Address = address;
        branch.Phone = phone;
    }

    private static BranchDto ToDto(Branch branch) => new()
    {
        Id = branch.Id,
        Name = branch.Name,
        Address = branch.Address,
        Phone = branch.Phone
    };

    private void RequireOwner()
    {
        if (!currentUser.IsAuthenticated || !currentUser.IsOwner)
            throw new UnauthorizedAccessException();
    }
}

