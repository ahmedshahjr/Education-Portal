using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Domain.Common;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EducationPortal.Infrastructure.Data.Interceptors;

/// <summary>
/// Writes a <see cref="ChangeAuditEntry"/> for every added, changed or removed commercial row (ADR-016): subscriptions
/// and their modules, invoices and payments, add-on requests, the module catalogue and its dependencies, plans and price
/// books. The entries are added to the same save, so the change and its record are committed together or not at all.
/// Use cases need no code for it; a new commercial entity is audited by adding its type to <see cref="AuditedTypes"/>.
/// </summary>
public sealed class ChangeAuditInterceptor : SaveChangesInterceptor
{
    /// <summary>The audited entity types.</summary>
    public static IReadOnlySet<Type> AuditedTypes { get; } = new HashSet<Type>
    {
        typeof(Subscription),
        typeof(TenantModule),
        typeof(Invoice),
        typeof(InvoiceLine),
        typeof(Payment),
        typeof(AddOnRequest),
        typeof(PortalModule),
        typeof(ModuleDependency),
        typeof(Plan),
        typeof(PlanModule),
        typeof(PriceBook),
        typeof(PriceBookItem),
    };

    // Kept by the auditable-entity interceptor already; recording them again in every entry adds nothing.
    private static readonly HashSet<string> IgnoredProperties = new(StringComparer.Ordinal)
    {
        nameof(BaseEntity.Id),
        nameof(BaseAuditableEntity.Created),
        nameof(BaseAuditableEntity.CreatedBy),
        nameof(BaseAuditableEntity.LastModified),
        nameof(BaseAuditableEntity.LastModifiedBy),
    };

    private readonly IUser _user;
    private readonly TimeProvider _timeProvider;

    public ChangeAuditInterceptor(IUser user, TimeProvider timeProvider)
    {
        _user = user;
        _timeProvider = timeProvider;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        ArgumentNullException.ThrowIfNull(eventData);
        AddEntries(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(eventData);
        AddEntries(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void AddEntries(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var changed = context.ChangeTracker.Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted && AuditedTypes.Contains(e.Metadata.ClrType))
            .ToList();

        if (changed.Count == 0)
        {
            return;
        }

        var now = _timeProvider.GetUtcNow();
        var actor = _user.Id;
        var traceId = Activity.Current?.TraceId.ToString();

        foreach (var entry in changed)
        {
            var changes = Describe(entry);
            if (changes.Count == 0)
            {
                continue;
            }

            context.Add(new ChangeAuditEntry
            {
                OccurredAt = now,
                ActorUserId = actor,
                SchoolId = entry.Metadata.FindProperty("SchoolId") is null ? null : (int?)entry.Property("SchoolId").CurrentValue,
                EntityType = entry.Metadata.ClrType.Name,
                EntityId = ((BaseEntity)entry.Entity).PublicId,
                Action = entry.State switch
                {
                    EntityState.Added => ChangeAuditAction.Added,
                    EntityState.Deleted => ChangeAuditAction.Removed,
                    _ => ChangeAuditAction.Modified,
                },
                Changes = JsonSerializer.Serialize(changes),
                TraceId = traceId,
            });
        }
    }

    /// <summary>The changed values of one row: every value when added or removed, the changed ones when modified.</summary>
    public static List<PropertyChange> Describe(EntityEntry entry)
    {
        var changes = new List<PropertyChange>();

        foreach (var property in entry.Properties)
        {
            var name = property.Metadata.Name;
            if (IgnoredProperties.Contains(name) || property.Metadata.IsShadowProperty())
            {
                continue;
            }

            switch (entry.State)
            {
                case EntityState.Added:
                    changes.Add(new PropertyChange(name, null, Format(property.CurrentValue)));
                    break;
                case EntityState.Deleted:
                    changes.Add(new PropertyChange(name, Format(property.OriginalValue), null));
                    break;
                case EntityState.Modified when property.IsModified && !Equals(property.OriginalValue, property.CurrentValue):
                    changes.Add(new PropertyChange(name, Format(property.OriginalValue), Format(property.CurrentValue)));
                    break;
            }
        }

        return changes;
    }

    /// <summary>A value as culture-free text: dates in round-trip ISO 8601 UTC, decimals without trailing zeros, numbers and enums as their invariant text.</summary>
    public static string? Format(object? value) => value switch
    {
        null => null,
        DateTimeOffset date => date.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture),
        DateTime date => date.ToString("O", CultureInfo.InvariantCulture),
        // Without trailing zeros, so 25000 read from a numeric(18,2) column and 25000 computed in code are the same text.
        decimal amount => amount.ToString("0.############################", CultureInfo.InvariantCulture),
        IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString(),
    };

    /// <summary>One changed property in an entry's <see cref="ChangeAuditEntry.Changes"/>.</summary>
    public sealed record PropertyChange(
        [property: System.Text.Json.Serialization.JsonPropertyName("property")] string Property,
        [property: System.Text.Json.Serialization.JsonPropertyName("old")] string? Old,
        [property: System.Text.Json.Serialization.JsonPropertyName("new")] string? New);
}
