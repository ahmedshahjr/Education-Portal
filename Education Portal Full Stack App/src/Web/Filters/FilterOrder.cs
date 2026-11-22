namespace EducationPortal.Web.Filters;

/// <summary>
/// The order of the global controller filters (ADR-025). A lower number runs first, so the first filter is the outermost.
/// The antiforgery attribute is 0: it runs before all of them, as the endpoint filter did.
/// The request rules (authorisation and validation) are not filters: each action calls IRequestRules with its final request.
/// </summary>
public static class FilterOrder
{
    public const int Antiforgery = 0;
    public const int Logging = 1;
    public const int UnhandledException = 2;
    public const int Performance = 3;
}
