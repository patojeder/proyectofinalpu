namespace VentaStock.Application.Contracts;

public interface ICurrentUser
{
    int? UserId { get; }
    bool IsAuthenticated { get; }
    bool IsOwner { get; }
    bool HasPermission(string permission);
}
