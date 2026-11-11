using System.Diagnostics;
using System.Globalization;
using EducationPortal.Application.Common.Diagnostics;
using EducationPortal.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EducationPortal.Infrastructure.Observability;

/// <summary>
/// Puts the request's <c>tenant.id</c> (its SchoolId, from <see cref="ICurrentTenant"/>) on the request's span and in
/// a logging scope, so every log line of the request carries it. Runs straight after tenant resolution. HTTP metrics
/// get the label only when <see cref="ObservabilityOptions.TenantIdAsMetricLabel"/> is on. Requests without a tenant,
/// such as health probes, are left as they are.
/// </summary>
public sealed class TenantTelemetryMiddleware(
    RequestDelegate next,
    ILogger<TenantTelemetryMiddleware> logger,
    IOptions<ObservabilityOptions> options)
{
    public async Task InvokeAsync(HttpContext context, ICurrentTenant tenant)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(tenant);

        if (tenant.SchoolId is not { } schoolId)
        {
            await next(context);
            return;
        }

        var tenantId = schoolId.ToString(CultureInfo.InvariantCulture);

        var activity = context.Features.Get<IHttpActivityFeature>()?.Activity ?? Activity.Current;
        activity?.SetTag(TelemetryNames.Tags.TenantId, tenantId);

        if (options.Value.TenantIdAsMetricLabel)
        {
            context.Features.Get<IHttpMetricsTagsFeature>()?.Tags.Add(new(TelemetryNames.Tags.TenantId, tenantId));
        }

        using (logger.BeginScope(new Dictionary<string, object?> { [TelemetryNames.Tags.TenantId] = tenantId }))
        {
            await next(context);
        }
    }
}
