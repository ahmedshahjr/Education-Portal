using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EducationPortal.Web.Controllers;

/// <summary>
/// Base class of the API controllers. Each controller sets its own route, <c>/api/{Name}</c>, and keeps the names of the
/// former handlers as action names, so the generated client and the OpenAPI operation ids do not change.
/// </summary>
/// <remarks>
/// ADR-025: a controller takes the request record as its action argument, checks it with <c>IRequestRules</c> and calls the
/// use case. No mediator library is used. The automatic 400 of <see cref="ApiControllerAttribute"/> is off
/// (SuppressModelStateInvalidFilter), so a request that does not bind is refused here with the same ProblemDetails as before.
/// </remarks>
[ApiController]
public abstract class ApiControllerBase : Controller
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!ModelState.IsValid)
        {
            // engineering-exception: ENG-EF-06 false positive: assigns the action result on the filter context; nothing is blocked on a task
            context.Result = ValidationProblem(ModelState);
        }

        base.OnActionExecuting(context);
    }
}
