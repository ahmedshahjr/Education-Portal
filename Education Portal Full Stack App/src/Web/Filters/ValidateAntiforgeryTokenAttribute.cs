using Microsoft.AspNetCore.Mvc;

namespace EducationPortal.Web.Filters;

/// <summary>
/// Put on a controller (or an action) whose unsafe requests need the antiforgery token (SEC-WEB-03). It runs before the
/// global filters, as the endpoint check did before the use case, so a request without the token never reaches a use case.
/// </summary>
public sealed class ValidateAntiforgeryTokenAttribute : TypeFilterAttribute
{
    public ValidateAntiforgeryTokenAttribute() : base(typeof(AntiforgeryTokenFilter))
    {
        Order = FilterOrder.Antiforgery;
    }
}
