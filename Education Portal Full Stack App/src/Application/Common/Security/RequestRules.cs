using EducationPortal.Application.Common.Validation;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace EducationPortal.Application.Common.Security;

/// <summary>
/// The rules a request must pass before its use case runs (ADR-025): the sign-in, master-tenant, module, role, right and
/// policy checks named by <see cref="AuthorizeAttribute"/> on the request type, then the request's FluentValidation
/// validators. A controller action calls <see cref="CheckAsync"/> with the request it is about to run, so the rules see
/// the request as it will run (a route id is already in it), and a denial is the same error as before.
/// </summary>
public interface IRequestRules
{
    Task CheckAsync(object request, CancellationToken cancellationToken);
}

public sealed class RequestRules(IRequestAuthorizer authorizer, IServiceProvider services) : IRequestRules
{
    public async Task CheckAsync(object request, CancellationToken cancellationToken)
    {
        var requestType = request.GetType();

        await authorizer.AuthorizeAsync(requestType, cancellationToken);

        var validators = services.GetServices(typeof(IValidator<>).MakeGenericType(requestType)).Cast<IValidator>();
        await RequestValidation.ValidateAsync(validators, request, cancellationToken);
    }
}
