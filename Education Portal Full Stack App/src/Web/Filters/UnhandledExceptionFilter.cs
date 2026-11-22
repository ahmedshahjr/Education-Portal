using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EducationPortal.Web.Filters;

/// <summary>
/// Logs an exception that an action raised and that no later filter handled, then lets it continue to the exception handler.
/// The request itself is not logged, because it can carry personal data (SEC-LOG-02, SEC-PII-02).
/// </summary>
public sealed class UnhandledExceptionFilter(ILoggerFactory loggerFactory) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var executed = await next();

        if (context.ActionDescriptor is ControllerActionDescriptor action
            && executed.Exception is { } exception && !executed.ExceptionHandled)
        {
            loggerFactory.CreateLogger(action.ControllerTypeInfo.FullName!).LogError(
                exception, "EducationPortal Request: Unhandled Exception for Request {RequestName}", action.MethodInfo.Name);
        }
    }
}
