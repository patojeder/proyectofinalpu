using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using BCrypt.Net;
using VentaStock.Application.Contracts;
using VentaStock.Application.DTOs;
using VentaStock.Domain.Contracts;

namespace VentaStock.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public AuthService(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<UserSessionDto?> ValidateCredentialsAsync(LoginRequestDto request)
    {
        var usuario = await _userRepository.GetByUserNameAsync(request.UserName);

        if (usuario == null)
        {
            return null;
        }

        bool passwordValida = BCrypt.Net.BCrypt.Verify(request.Password, usuario.PasswordHash);
        if (!passwordValida)
        {
            return null;
        }

        return MapSession(usuario);
    }

    public async Task<UserSessionDto?> GetUserSessionAsync(int userId)
    {
        var usuario = await _userRepository.GetSessionByIdAsync(userId);
        return usuario == null ? null : MapSession(usuario);
    }

    public async Task CompleteTemporaryPasswordChangeAsync(int userId, string newPassword)
    {
        var user = await _userRepository.GetSessionByIdAsync(userId);
        if (user is null || !user.MustChangePassword)
            throw new InvalidOperationException("La cuenta no requiere cambiar una contraseña temporal.");

        if (BCrypt.Net.BCrypt.Verify(newPassword, user.PasswordHash))
            throw new InvalidOperationException("La nueva contraseña debe ser distinta de la temporal.");

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        if (!await _userRepository.CompleteTemporaryPasswordChangeAsync(userId, passwordHash))
            throw new InvalidOperationException("No se pudo actualizar la contraseña. Iniciá sesión nuevamente.");
    }

    private UserSessionDto MapSession(VentaStock.Domain.Entities.User user)
    {
        var session = _mapper.Map<UserSessionDto>(user);
        session.CredentialVersion = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(user.PasswordHash)));
        return session;
    }
}
