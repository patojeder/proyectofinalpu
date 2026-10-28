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

        return _mapper.Map<UserSessionDto>(usuario);
    }

    public async Task<UserSessionDto?> GetUserSessionAsync(int userId)
    {
        var usuario = await _userRepository.GetSessionByIdAsync(userId);
        return usuario == null ? null : _mapper.Map<UserSessionDto>(usuario);
    }
}
