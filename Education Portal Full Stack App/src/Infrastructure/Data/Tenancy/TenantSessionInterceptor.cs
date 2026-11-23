using System.Data.Common;
using EducationPortal.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EducationPortal.Infrastructure.Data.Tenancy;

/// <summary>
/// Tells PostgreSQL which tenant the connection works for (ADR-026). Every time a physical connection is opened, this
/// interceptor sets two session settings that the row-level security policies read:
/// <c>app.tenant_id</c> (the current tenant, or empty when there is none, which matches no tenant-owned row) and
/// <c>app.tenant_bypass</c> (<c>on</c> only for a master session or an open <see cref="TenantSystemScope"/>).
/// Pooled connections are set again on every open, so a connection never carries the tenant of an earlier request.
/// </summary>
public sealed class TenantSessionInterceptor(ICurrentTenant tenant, TenantSystemScope systemScope) : DbConnectionInterceptor
{
    /// <summary>The session setting that holds the current tenant id.</summary>
    public const string TenantSetting = "app.tenant_id";

    /// <summary>The session setting that lifts the row-level security for a master session or a system scope.</summary>
    public const string BypassSetting = "app.tenant_bypass";

    private const string SetCommand = "SELECT set_config('app.tenant_id', @tenant, false), set_config('app.tenant_bypass', @bypass, false)";

    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    {
        ArgumentNullException.ThrowIfNull(connection);
        Apply(connection);
    }

    public override async Task ConnectionOpenedAsync(DbConnection connection, ConnectionEndEventData eventData, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(connection);
        await ApplyAsync(connection, cancellationToken);
    }

    private void Apply(DbConnection connection)
    {
        using var command = CreateCommand(connection);
        command.ExecuteNonQuery();
    }

    private async Task ApplyAsync(DbConnection connection, CancellationToken cancellationToken)
    {
        await using var command = CreateCommand(connection);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private DbCommand CreateCommand(DbConnection connection)
    {
        var command = connection.CreateCommand();
        command.CommandText = SetCommand;

        var tenantParameter = command.CreateParameter();
        tenantParameter.ParameterName = "tenant";
        // Outside a resolved tenant the setting is empty, which matches no tenant-owned row (fail closed).
        tenantParameter.Value = tenant.IsResolved ? tenant.TenantId.ToString(System.Globalization.CultureInfo.InvariantCulture) : string.Empty;
        command.Parameters.Add(tenantParameter);

        var bypassParameter = command.CreateParameter();
        bypassParameter.ParameterName = "bypass";
        bypassParameter.Value = tenant.BypassesTenantFilters || systemScope.IsActive ? "on" : "off";
        command.Parameters.Add(bypassParameter);

        return command;
    }
}
