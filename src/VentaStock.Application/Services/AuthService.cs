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

    public async Task<UsuarioSesionDto?> ValidarCredencialesAsync(LoginRequestDto request)
    {
        var usuario = await _userRepository.GetByUsernameAsync(request.NombreUsuario);

        if (usuario == null)
        {
            return null;
        }

        bool passwordValida = BCrypt.Net.BCrypt.Verify(request.Password, usuario.PasswordHash);
        if (!passwordValida)
        {
            return null;
        }

        return _mapper.Map<UsuarioSesionDto>(usuario);
    }
}