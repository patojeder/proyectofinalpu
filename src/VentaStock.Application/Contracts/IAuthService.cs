using VentaStock.Application.DTOs;

namespace VentaStock.Application.Contracts;

public interface IAuthService
{
    Task<UserSessionDto?> ValidateCredentialsAsync(LoginRequestDto request);
    Task<UserSessionDto?> GetUserSessionAsync(int userId);
    Task CompleteTemporaryPasswordChangeAsync(int userId, string newPassword);
}
