namespace EducationPortal.Application.Common.Security;

/// <summary>
/// Applies the <see cref="AuthorizeAttribute"/> rules of a request to the current user and tenant (ADR-025). Throws the same
/// exceptions as the former authorisation pipeline step, and returns when the request may run.
/// </summary>
public interface IRequestAuthorizer
{
    Task AuthorizeAsync(Type requestType, CancellationToken cancellationToken);
}
