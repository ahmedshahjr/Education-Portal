using System.Diagnostics;
using EducationPortal.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EducationPortal.Web.Filters;

/// <summary>
/// Warns about actions that take longer than 500 ms, after the action has completed. A request that failed is not timed,
/// as before. It logs the action name and user id only, never the request (SEC-LOG-02, SEC-PII-02).
/// </summary>
public sealed class PerformanceFilter(ILoggerFactory loggerFactory, IUser user) : IAsyncActionFilter
{
    private const long SlowRequestMilliseconds = 500;

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.ActionDescriptor is not ControllerActionDescriptor action)
        {
            await next();
            return;
        }

        var timer = Stopwatch.StartNew();

        var executed = await next();

        timer.Stop();

        if (executed.Exception is null && timer.ElapsedMilliseconds > SlowRequestMilliseconds)
        {
            loggerFactory.CreateLogger(action.ControllerTypeInfo.FullName!).LogWarning(
                "EducationPortal Long Running Request: {RequestName} ({ElapsedMilliseconds} milliseconds) {UserId}",
                action.MethodInfo.Name, timer.ElapsedMilliseconds, user.Id ?? string.Empty);
        }
    }
}
