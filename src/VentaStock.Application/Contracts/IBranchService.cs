using VentaStock.Application.DTOs;

namespace VentaStock.Application.Contracts;

public interface IBranchService
{
    Task<IEnumerable<BranchDto>> GetAllAsync(string? searchTerm = null);
    Task<EditBranchDto?> GetForEditByIdAsync(int id);
    Task<BranchDto?> GetForDeleteByIdAsync(int id);
    Task CreateAsync(CreateBranchDto dto);
    Task UpdateAsync(EditBranchDto dto);
    Task DeleteAsync(int id);
}

