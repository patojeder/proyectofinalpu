using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace VentaStock.Web.Security;

public sealed class UserAccessDeniedAttribute : ExceptionFilterAttribute
{
    public override void OnException(ExceptionContext context)
    {
        if (context.Exception is UnauthorizedAccessException)
        {
            context.Result = new ForbidResult();
            context.ExceptionHandled = true;
        }
    }
}
