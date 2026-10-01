using VentaStock.Application.DTOs;

namespace VentaStock.Application.Contracts;

public interface IAuthService
{
    Task<UsuarioSesionDto?> ValidarCredencialesAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
}