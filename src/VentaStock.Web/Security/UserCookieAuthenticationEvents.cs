using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using VentaStock.Application.Contracts;

namespace VentaStock.Web.Security;

public class UserCookieAuthenticationEvents : CookieAuthenticationEvents
{
    private readonly IAuthService _authService;

    public UserCookieAuthenticationEvents(IAuthService authService) => _authService = authService;

    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        var idText = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(idText, out var userId))
        {
            await RejectAsync(context);
            return;
        }

        var user = await _authService.GetUserSessionAsync(userId);
        if (user is null)
        {
            await RejectAsync(context);
            return;
        }

        var freshPrincipal = UserClaimsPrincipalFactory.Create(user);
        if (UserClaimsPrincipalFactory.HasSameAccess(context.Principal!, freshPrincipal))
            return;

        context.ReplacePrincipal(freshPrincipal);
        context.ShouldRenew = true;
    }

    private static async Task RejectAsync(CookieValidatePrincipalContext context)
    {
        context.RejectPrincipal();
        await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }
}
