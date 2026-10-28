namespace VentaStock.Web.Security;

public class TemporaryPasswordChangeMiddleware
{
    private readonly RequestDelegate _next;

    public TemporaryPasswordChangeMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.HasClaim("MustChangePassword", "true")
            && !context.Request.Path.StartsWithSegments("/Auth/ChangePassword", StringComparison.OrdinalIgnoreCase)
            && !context.Request.Path.StartsWithSegments("/Auth/Logout", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.Redirect("/Auth/ChangePassword");
            return;
        }

        await _next(context);
    }
}
