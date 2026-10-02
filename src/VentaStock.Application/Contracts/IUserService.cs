using VentaStock.Application.DTOs;

namespace VentaStock.Application.Contracts;

public interface IUserService
{
    Task<IEnumerable<UserDto>> GetAllAsync();
    Task<UserDto?> GetByIdAsync(int userId);
    Task<EditUserDto?> GetForEditByIdAsync(int userId);
    Task<DeleteUserDto?> GetForDeleteByIdAsync(int userId);
    Task CreateAsync(CreateUserDto dto);
    Task UpdateAsync(EditUserDto dto);
    Task DeleteAsync(int userId);
    Task<IEnumerable<RoleSelectDto>> GetRolesForSelectAsync();
}