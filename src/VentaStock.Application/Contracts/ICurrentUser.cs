namespace VentaStock.Application.Contracts;

public interface ICurrentUser
{
    bool IsAuthenticated { get; }
    bool IsOwner { get; }
    bool HasPermission(string permission);
}
