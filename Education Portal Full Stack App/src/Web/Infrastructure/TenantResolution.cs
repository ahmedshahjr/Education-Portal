using System.Globalization;
using System.Security.Claims;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Enums;
using EducationPortal.Web.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace EducationPortal.Web.Infrastructure;

/// <summary>
/// Multi-tenancy by domain (AGENTS.md section 2). <see cref="UseTenantResolution"/> finds the school that owns the
/// request's host and answers 404 for unknown hosts, without serving anything (SEC-AUTHZ-10).
/// <see cref="UseTenantSessionCheck"/> makes sure a session is used only on its own school's domain.
/// </summary>
public static class TenantResolution
{
    // Health probes come from the hosting platform, which does not use a school's domain. They return no data.
    private static readonly PathString[] HostIndependentPaths = ["/health", "/alive"];

    public static IApplicationBuilder UseTenantResolution(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            if (HostIndependentPaths.Any(path => context.Request.Path.StartsWithSegments(path)))
            {
                await next();
                return;
            }

            var resolver = context.RequestServices.GetRequiredService<ITenantResolver>();
            var tenant = await resolver.ResolveAsync(context.Request.Host.Host, context.RequestAborted);

            if (tenant is null)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }

            context.RequestServices.GetRequiredService<CurrentTenant>().Set(tenant);

            await next();
        });
    }

    /// <summary>
    /// Runs after authentication. A session whose school is not the school of the host is treated as no session, and
    /// the attempt is audited (SEC-LOG-03). Browsers already keep the <c>__Host-</c> cookie on one host, so this catches
    /// a cookie that was copied to another school's domain.
    /// </summary>
    public static IApplicationBuilder UseTenantSessionCheck(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var tenant = context.RequestServices.GetRequiredService<ICurrentTenant>();
                var sessionSchoolId = context.User.FindFirstValue(TenantClaimTypes.SchoolId);
                var hostSchoolId = tenant.SchoolId?.ToString(CultureInfo.InvariantCulture);

                if (hostSchoolId is null || sessionSchoolId != hostSchoolId)
                {
                    await context.RequestServices.GetRequiredService<ISecurityAuditLog>().WriteAsync(
                        SecurityAuditEventType.TenantMismatch,
                        SecurityAuditOutcome.Failure,
                        targetType: "School",
                        targetId: hostSchoolId);

                    context.User = new ClaimsPrincipal(new ClaimsIdentity());
                }
            }

            await next();
        });
    }
}
