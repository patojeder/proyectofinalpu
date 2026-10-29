using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.WebUtilities;
using VentaStock.Application.Contracts;

namespace VentaStock.Web.Security;

public class UserCookieAuthenticationEvents : CookieAuthenticationEvents
{
    private const string SessionInvalidatedItemKey = "VentaStock.SessionInvalidated";
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
        if (!UserClaimsPrincipalFactory.HasSameCredential(context.Principal!, freshPrincipal))
        {
            await RejectAsync(context, "credentialsChanged");
            return;
        }

        if (UserClaimsPrincipalFactory.HasSameAccess(context.Principal!, freshPrincipal))
            return;

        context.ReplacePrincipal(freshPrincipal);
        context.ShouldRenew = true;
    }

    public override Task RedirectToLogin(RedirectContext<CookieAuthenticationOptions> context)
    {
        if (!context.HttpContext.Items.TryGetValue(SessionInvalidatedItemKey, out var status)
            || status is not string sessionStatus)
            return base.RedirectToLogin(context);

        var loginUrl = QueryHelpers.AddQueryString(context.RedirectUri, "sessionStatus", sessionStatus);
        context.Response.Redirect(loginUrl);
        return Task.CompletedTask;
    }

    private static async Task RejectAsync(CookieValidatePrincipalContext context, string sessionStatus = "invalidated")
    {
        context.HttpContext.Items[SessionInvalidatedItemKey] = sessionStatus;
        context.RejectPrincipal();
        await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }
}
