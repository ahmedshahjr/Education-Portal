using System.Diagnostics;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using EducationPortal.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EducationPortal.Infrastructure.Identity;

/// <summary>
/// Writes security audit records (SEC-LOG-03, SEC-LOG-04). Each record goes through its own scope and unit of
/// work, so it is kept even when the request fails afterwards. Records hold ids and technical facts only.
/// </summary>
internal sealed class SecurityAuditLog : ISecurityAuditLog
{
    private const int MaxUserAgentLength = 512;
    private const int MaxTraceIdLength = 64;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUser _user;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<SecurityAuditLog> _logger;

    public SecurityAuditLog(
        IServiceScopeFactory scopeFactory,
        IHttpContextAccessor httpContextAccessor,
        IUser user,
        TimeProvider timeProvider,
        ILogger<SecurityAuditLog> logger)
    {
        _scopeFactory = scopeFactory;
        _httpContextAccessor = httpContextAccessor;
        _user = user;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task WriteAsync(
        SecurityAuditEventType eventType,
        SecurityAuditOutcome outcome,
        string? actorUserId = null,
        string? targetType = null,
        string? targetId = null)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var actor = actorUserId ?? _user.Id;

        var record = new SecurityAuditEvent
        {
            OccurredAt = _timeProvider.GetUtcNow(),
            EventType = eventType,
            Outcome = outcome,
            ActorUserId = actor,
            TargetType = targetType,
            TargetId = targetId,
            SourceIp = httpContext?.Connection.RemoteIpAddress?.ToString(),
            UserAgent = Truncate(httpContext?.Request.Headers["User-Agent"].ToString(), MaxUserAgentLength),
            TraceId = Truncate(Activity.Current?.TraceId.ToString() ?? httpContext?.TraceIdentifier, MaxTraceIdLength),
        };

        // CancellationToken.None keeps the record even when the client has already disconnected.
        await using var scope = _scopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        context.SecurityAuditEvents.Add(record);
        await context.SaveChangesAsync(CancellationToken.None);

        _logger.LogInformation(
            "Security audit event {EventType} with outcome {Outcome} for actor {ActorUserId}",
            eventType,
            outcome,
            actor ?? "anonymous");
    }

    private static string? Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        return value.Length <= maxLength ? value : value[..maxLength];
    }
}
