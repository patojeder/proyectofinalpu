using VentaStock.Application.DTOs;

namespace VentaStock.Application.Contracts;

public interface IAuthService
{
    Task<UserSessionDto?> ValidateCredentialsAsync(LoginRequestDto request);
}