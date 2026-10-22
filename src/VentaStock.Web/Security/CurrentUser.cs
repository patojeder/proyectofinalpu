using VentaStock.Domain.Security;
using VentaStock.Application.Contracts;

namespace VentaStock.Web.Security;

public sealed class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    public bool IsAuthenticated => accessor.HttpContext?.User.Identity?.IsAuthenticated == true;
    public bool IsOwner => IsAuthenticated && accessor.HttpContext!.User.IsInRole(SystemRoles.Owner);
    public bool HasPermission(string permission) => IsAuthenticated &&
        (IsOwner || accessor.HttpContext!.User.HasClaim("Permiso", permission));
}
