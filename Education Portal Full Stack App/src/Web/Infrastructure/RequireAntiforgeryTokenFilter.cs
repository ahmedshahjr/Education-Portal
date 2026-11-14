using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace EducationPortal.Web.Infrastructure;

/// <summary>
/// Rejects an unsafe request whose antiforgery token is missing or wrong, before the endpoint runs (SEC-WEB-03).
/// The antiforgery middleware alone does not stop a request: for JSON endpoints it only records the outcome, and the
/// endpoint still runs. So every endpoint group gets this filter in <see cref="WebApplicationExtensions.MapEndpoints"/>.
/// </summary>
public sealed class RequireAntiforgeryTokenFilter(IAntiforgery antiforgery) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var httpContext = context.HttpContext;

        if (IsSafeMethod(httpContext.Request.Method))
        {
            return await next(context);
        }

        // The antiforgery middleware has already checked the request and recorded the outcome. When that outcome is
        // invalid, reading the form throws InvalidOperationException, so the recorded outcome is checked first.
        if (httpContext.Features.Get<IAntiforgeryValidationFeature>() is { IsValid: false })
        {
            return Reject();
        }

        try
        {
            // Throws when the header or the cookie is missing, or when the token does not match. Any other exception is a
            // server fault, so it is not caught here and the exception handler answers it.
            await antiforgery.ValidateRequestAsync(httpContext);
        }
        catch (AntiforgeryValidationException)
        {
            return Reject();
        }

        return await next(context);
    }

    private static ProblemHttpResult Reject() => TypedResults.Problem(
        title: "The antiforgery token is missing or invalid.",
        statusCode: StatusCodes.Status400BadRequest,
        type: "https://tools.ietf.org/html/rfc9110#section-15.5.1");

    private static bool IsSafeMethod(string method) =>
        HttpMethods.IsGet(method) || HttpMethods.IsHead(method) || HttpMethods.IsOptions(method) || HttpMethods.IsTrace(method);
}
