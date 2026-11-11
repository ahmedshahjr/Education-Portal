using EducationPortal.Infrastructure.Observability;

namespace Microsoft.AspNetCore.Builder;

public static class ObservabilityApplicationBuilderExtensions
{
    /// <summary>
    /// Adds <c>tenant.id</c> to the request's span and log scope (<see cref="TenantTelemetryMiddleware"/>). Call it
    /// straight after <c>UseTenantResolution</c>.
    /// </summary>
    public static IApplicationBuilder UseTenantTelemetry(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        return app.UseMiddleware<TenantTelemetryMiddleware>();
    }
}
