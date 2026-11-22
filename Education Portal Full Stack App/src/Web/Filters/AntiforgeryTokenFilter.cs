using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EducationPortal.Web.Filters;

/// <summary>
/// Rejects an unsafe request whose antiforgery token is missing or wrong, with the same 400 as the former endpoint filter
/// (SEC-WEB-03). The antiforgery middleware only records the outcome, so the recorded outcome and the token are both checked.
/// </summary>
public sealed class AntiforgeryTokenFilter(IAntiforgery antiforgery) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var httpContext = context.HttpContext;

        if (IsSafeMethod(httpContext.Request.Method))
        {
            await next();
            return;
        }

        if (httpContext.Features.Get<IAntiforgeryValidationFeature>() is { IsValid: false } || !await TokenIsValidAsync(httpContext))
        {
            // engineering-exception: ENG-EF-06 false positive: assigns the action result on the filter context; nothing is blocked on a task
            context.Result = new ObjectResult(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "The antiforgery token is missing or invalid.",
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            })
            {
                StatusCode = StatusCodes.Status400BadRequest,
            };
            return;
        }

        await next();
    }

    private async Task<bool> TokenIsValidAsync(HttpContext httpContext)
    {
        try
        {
            // Throws when the header or the cookie is missing, or when the token does not match. Any other exception is a
            // server fault, so it is not caught here and the exception handler answers it.
            await antiforgery.ValidateRequestAsync(httpContext);
            return true;
        }
        catch (AntiforgeryValidationException)
        {
            return false;
        }
    }

    private static bool IsSafeMethod(string method) =>
        HttpMethods.IsGet(method) || HttpMethods.IsHead(method) || HttpMethods.IsOptions(method) || HttpMethods.IsTrace(method);
}
