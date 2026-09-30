using AutoMapper;
using BCrypt.Net;
using VentaStock.Application.Contracts;
using VentaStock.Application.DTOs;
using VentaStock.Domain.Contracts;

namespace VentaStock.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IMapper _mapper;

    public AuthService(IUsuarioRepository usuarioRepository, IMapper mapper)
    {
        _usuarioRepository = usuarioRepository;
        _mapper = mapper;
    }

    public async Task<UsuarioSesionDto?> ValidarCredencialesAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var usuario = await _usuarioRepository.ObtenerPorNombreUsuarioAsync(request.NombreUsuario, cancellationToken);

        if (usuario == null || !usuario.Activo)
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