using AutoMapper;
using BCrypt.Net;
using VentaStock.Application.Contracts;
using VentaStock.Application.DTOs;
using VentaStock.Domain.Contracts;
using VentaStock.Domain.Entities;

namespace VentaStock.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IMapper _mapper;

    public UserService(IUserRepository userRepository, IRoleRepository roleRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<RoleSelectDto>> GetRolesForSelectAsync()
    {
        var roles = await _roleRepository.GetAllAsync();
        return roles.Select(r => new RoleSelectDto
        {
            IdRol = r.IdRol,
            Nombre = r.Nombre
        });
    }

    public async Task<IEnumerable<UserDto>> GetAllAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<UserDto>>(users);
    }

    public async Task<UserDto?> GetByIdAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        return user is null ? null : _mapper.Map<UserDto>(user);
    }

    public async Task<EditUserDto?> GetForEditByIdAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        return user is null ? null : _mapper.Map<EditUserDto>(user);
    }

    public async Task<DeleteUserDto?> GetForDeleteByIdAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        return user is null ? null : _mapper.Map<DeleteUserDto>(user);
    }

    public async Task CreateAsync(CreateUserDto dto)
    {
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
            Activo = true,
            FechaCreacion = DateTime.UtcNow
        };

        await _userRepository.CreateAsync(user);
    }

    public async Task UpdateAsync(EditUserDto dto)
    {
        var existingUser = await _userRepository.GetByIdAsync(dto.IdUsuario);
        if (existingUser is null)
            throw new InvalidOperationException($"El usuario con ID {dto.IdUsuario} no existe.");

        if (await _userRepository.ExistsUsernameAsync(dto.NombreUsuario.Trim(), excludeId: dto.IdUsuario))
            throw new InvalidOperationException("El nombre de usuario ya está siendo utilizado por otra cuenta.");

        if (await _userRepository.ExistsMailAsync(dto.Mail.Trim(), excludeId: dto.IdUsuario))
            throw new InvalidOperationException("El correo electrónico ya está siendo utilizado por otra cuenta.");

        existingUser.NombreUsuario = dto.NombreUsuario.Trim();
        existingUser.Mail = dto.Mail.Trim();
        existingUser.IdRol = dto.IdRol;

        if (!string.IsNullOrWhiteSpace(dto.NewPassword))
        {
            existingUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        }

        await _userRepository.UpdateAsync(existingUser);
    }

    public async Task DeleteAsync(int userId)
    {
        var existingUser = await _userRepository.GetByIdAsync(userId);
        if (existingUser is null)
            throw new InvalidOperationException($"El usuario con ID {userId} no existe.");

        await _userRepository.DeleteAsync(userId);
    }
}