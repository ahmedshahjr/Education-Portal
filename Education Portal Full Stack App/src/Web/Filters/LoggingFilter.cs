using EducationPortal.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EducationPortal.Web.Filters;

/// <summary>
/// Logs which action runs and for which user id, under the controller's log category (ADR-025). The request and the user's
/// name or email are never logged (SEC-LOG-02, SEC-PII-02).
/// </summary>
public sealed class LoggingFilter(ILoggerFactory loggerFactory, IUser user) : IAsyncActionFilter
{
    public Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.ActionDescriptor is ControllerActionDescriptor action)
        {
            loggerFactory.CreateLogger(action.ControllerTypeInfo.FullName!).LogInformation(
                "EducationPortal Request: {RequestName} {UserId}", action.MethodInfo.Name, user.Id ?? string.Empty);
        }

        return next();
    }
}
