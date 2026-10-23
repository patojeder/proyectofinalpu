using VentaStock.Domain.Security;
using AutoMapper;
using BCrypt.Net;
using VentaStock.Application.Contracts;
using VentaStock.Application.DTOs;
using VentaStock.Domain.Contracts;
using VentaStock.Domain.Entities;
using VentaStock.Domain.Security.Permissions;

namespace VentaStock.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IMapper _mapper;
    private readonly ICurrentUser _currentUser;

    public UserService(IUserRepository userRepository, IRoleRepository roleRepository, IMapper mapper, ICurrentUser currentUser)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<RoleSelectDto>> GetRolesForSelectAsync()
    {
        if (!_currentUser.HasPermission(UserPermissions.Create) && !_currentUser.HasPermission(UserPermissions.Update))
            throw new UnauthorizedAccessException();
        var roles = await _roleRepository.GetAllAsync();
        roles = roles.Where(r => _currentUser.IsOwner || !IsOwnerRole(r));
        return roles.Select(r => new RoleSelectDto
        {
            IdRol = r.IdRol,
            Nombre = r.Nombre
        });
    }

    public async Task<IEnumerable<UserDto>> GetAllAsync()
    {
        RequirePermission(UserPermissions.Read);
        var users = await _userRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<UserDto>>(users);
    }

    public async Task<UserDto?> GetByIdAsync(int userId)
    {
        RequirePermission(UserPermissions.Read);
        var user = await _userRepository.GetByIdAsync(userId);
        return user is null ? null : _mapper.Map<UserDto>(user);
    }

    public async Task<EditUserDto?> GetForEditByIdAsync(int userId)
    {
        RequirePermission(UserPermissions.Update);
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is not null) RequireManageableUser(user);
        return user is null ? null : _mapper.Map<EditUserDto>(user);
    }

    public async Task<DeleteUserDto?> GetForDeleteByIdAsync(int userId)
    {
        RequirePermission(UserPermissions.Delete);
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is not null) RequireManageableUser(user);
        return user is null ? null : _mapper.Map<DeleteUserDto>(user);
    }

    public async Task CreateAsync(CreateUserDto dto)
    {
        RequirePermission(UserPermissions.Create);
        await RequireAssignableRoleAsync(dto.IdRol);
        if (await _userRepository.ExistsUsernameAsync(dto.NombreUsuario.Trim()))
            throw new InvalidOperationException("El nombre de usuario ingresado ya se encuentra registrado.");

        if (await _userRepository.ExistsMailAsync(dto.Mail.Trim()))
            throw new InvalidOperationException("El correo electrónico ingresado ya se encuentra registrado.");

        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        var user = new Usuario
        {
            NombreUsuario = dto.NombreUsuario.Trim(),
            Mail = dto.Mail.Trim(),
            IdRol = dto.IdRol,
            PasswordHash = hashedPassword,
            FechaCreacion = DateTime.UtcNow
        };

        await _userRepository.CreateAsync(user);
    }

    public async Task UpdateAsync(EditUserDto dto)
    {
        RequirePermission(UserPermissions.Update);
        var existingUser = await _userRepository.GetByIdAsync(dto.IdUsuario);
        if (existingUser is null)
            throw new InvalidOperationException($"El usuario con ID {dto.IdUsuario} no existe.");

        RequireManageableUser(existingUser);
        await RequireAssignableRoleAsync(dto.IdRol);
        if (!string.IsNullOrWhiteSpace(dto.NewPassword))
            RequirePermission(UserPermissions.ResetPassword);

        if (await _userRepository.ExistsUsernameAsync(dto.NombreUsuario.Trim(), excludeId: dto.IdUsuario))
            throw new InvalidOperationException("El nombre de usuario ya está siendo utilizado por otra cuenta.");

        if (await _userRepository.ExistsMailAsync(dto.Mail.Trim(), excludeId: dto.IdUsuario))
            throw new InvalidOperationException("El correo electrónico ya está siendo utilizado por otra cuenta.");

        var passwordHash = existingUser.PasswordHash;

        if (!string.IsNullOrWhiteSpace(dto.NewPassword))
        {
            passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        }

        // Conservar el rol de la entidad seguida por EF hasta validar el último Dueño.
        var proposedUser = new Usuario
        {
            IdUsuario = existingUser.IdUsuario,
            NombreUsuario = dto.NombreUsuario.Trim(),
            Mail = dto.Mail.Trim(),
            IdRol = dto.IdRol,
            PasswordHash = passwordHash
        };

        await _userRepository.UpdateAsync(proposedUser);
    }

    public async Task DeleteAsync(int userId)
    {
        RequirePermission(UserPermissions.Delete);
        var existingUser = await _userRepository.GetByIdAsync(userId);
        if (existingUser is null)
            throw new InvalidOperationException($"El usuario con ID {userId} no existe.");

        RequireManageableUser(existingUser);
        await _userRepository.DeleteAsync(userId);
    }

    private void RequirePermission(string permission)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.HasPermission(permission))
            throw new UnauthorizedAccessException();
    }

    private static bool IsOwnerRole(Rol role) =>
        string.Equals(role.Nombre, SystemRoles.Owner, StringComparison.OrdinalIgnoreCase);

    private void RequireManageableUser(Usuario user)
    {
        if (!_currentUser.IsOwner && IsOwnerRole(user.IdRolNavigation))
            throw new UnauthorizedAccessException();
    }

    private async Task RequireAssignableRoleAsync(int roleId)
    {
        var role = await _roleRepository.GetByIdAsync(roleId)
            ?? throw new InvalidOperationException("El rol seleccionado no existe.");
        if (!_currentUser.IsOwner && IsOwnerRole(role))
            throw new UnauthorizedAccessException();
    }

}
