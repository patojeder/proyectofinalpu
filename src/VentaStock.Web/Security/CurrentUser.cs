using VentaStock.Application.Contracts;
using VentaStock.Application.Security;

namespace VentaStock.Web.Security;

public sealed class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    public bool IsAuthenticated => accessor.HttpContext?.User.Identity?.IsAuthenticated == true;
    public bool IsOwner => IsAuthenticated && accessor.HttpContext!.User.IsInRole(UserPermissions.OwnerRole);
    public bool HasPermission(string permission) => IsAuthenticated &&
        (IsOwner || accessor.HttpContext!.User.HasClaim("Permiso", permission));
}
