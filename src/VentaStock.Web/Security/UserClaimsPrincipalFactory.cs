using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using VentaStock.Application.DTOs;

namespace VentaStock.Web.Security;

public static class UserClaimsPrincipalFactory
{
    public static ClaimsPrincipal Create(UserSessionDto user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.Role, user.Role)
        };
        claims.AddRange(user.Permissions.Select(permission => new Claim("Permission", permission)));
        return new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
    }

    public static bool HasSameAccess(ClaimsPrincipal current, ClaimsPrincipal updated)
    {
        return current.Identity?.Name == updated.Identity?.Name
            && current.FindFirst(ClaimTypes.NameIdentifier)?.Value == updated.FindFirst(ClaimTypes.NameIdentifier)?.Value
            && current.FindFirst(ClaimTypes.Role)?.Value == updated.FindFirst(ClaimTypes.Role)?.Value
            && current.FindAll("Permission").Select(claim => claim.Value).OrderBy(value => value)
                .SequenceEqual(updated.FindAll("Permission").Select(claim => claim.Value).OrderBy(value => value));
    }
}
