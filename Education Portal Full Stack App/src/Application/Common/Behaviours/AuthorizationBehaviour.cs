using System.Reflection;
using EducationPortal.Application.Common.Exceptions;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;

namespace EducationPortal.Application.Common.Behaviours;

public class AuthorizationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> 
    where TRequest : notnull
{
    private readonly IUser _user;
    private readonly IIdentityService _identityService;
    private readonly ICurrentTenant _tenant;
    private readonly IModuleAccess _moduleAccess;

    public AuthorizationBehaviour(
        IUser user,
        IIdentityService identityService,
        ICurrentTenant tenant,
        IModuleAccess moduleAccess)
    {
        _user = user;
        _identityService = identityService;
        _tenant = tenant;
        _moduleAccess = moduleAccess;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var authorizeAttributes = request.GetType().GetCustomAttributes<AuthorizeAttribute>();

        if (authorizeAttributes.Any())
        {
            // Must be authenticated user
            if (_user.Id == null)
            {
                throw new UnauthorizedAccessException();
            }

            // Master-tenant use cases run only on the master tenant's domain (SEC-AUTHZ-07)
            if (authorizeAttributes.Any(a => a.MasterTenant) && !_tenant.IsMaster)
            {
                throw new ForbiddenAccessException();
            }

            // Module switches per school (SEC-AUTHZ-01): a module the master tenant has not switched on for this school is
            // refused for everyone in it. The master tenant oversees every school, so modules never limit it.
            var modules = authorizeAttributes
                .Select(a => a.Module)
                .Where(m => !string.IsNullOrWhiteSpace(m))
                .Distinct(StringComparer.Ordinal)
                .ToList();

            if (modules.Count > 0 && !_tenant.IsMaster)
            {
                var schoolId = _tenant.SchoolId ?? throw new ForbiddenAccessException();

                foreach (var module in modules)
                {
                    if (!await _moduleAccess.IsEnabledAsync(schoolId, module, cancellationToken))
                    {
                        throw new ForbiddenAccessException();
                    }
                }
            }

            // Role-based authorization
            var authorizeAttributesWithRoles = authorizeAttributes.Where(a => !string.IsNullOrWhiteSpace(a.Roles));

            if (authorizeAttributesWithRoles.Any())
            {
                var authorized = false;

                foreach (var roles in authorizeAttributesWithRoles.Select(a => a.Roles.Split(',')))
                {
                    foreach (var role in roles)
                    {
                        var isInRole = _user.Roles?.Any(x => role == x)??false;
                        if (isInRole)
                        {
                            authorized = true;
                            break;
                        }
                    }
                }

                // Must be a member of at least one role in roles
                if (!authorized)
                {
                    throw new ForbiddenAccessException();
                }
            }

            // Policy-based authorization
            var authorizeAttributesWithPolicies = authorizeAttributes.Where(a => !string.IsNullOrWhiteSpace(a.Policy));
            if (authorizeAttributesWithPolicies.Any())
            {
                foreach (var policy in authorizeAttributesWithPolicies.Select(a => a.Policy))
                {
                    var authorized = await _identityService.AuthorizeAsync(_user.Id, policy);

                    if (!authorized)
                    {
                        throw new ForbiddenAccessException();
                    }
                }
            }
        }

        // User is authorized / authorization not required
        return await next();
    }
}
