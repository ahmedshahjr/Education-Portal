using System.Linq.Expressions;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EducationPortal.Infrastructure.Data.Lookups;

/// <summary>The lookup table for every enum column (ENG-ENUM-04). Add a line here when an entity gets an enum property.</summary>
public static class EnumLookupTables
{
    /// <summary>
    /// The lookup tables. They are reference data seeded with the schema, so anything that empties the database between
    /// tests (Respawn) leaves them alone.
    /// </summary>
    public static IReadOnlyList<string> Tables { get; } =
        ["SecurityAuditEventTypes", "SecurityAuditOutcomes", "BrandingImageKinds", "SchoolRequestStatuses", "StateTypes", "BillingCycles", "SubscriptionStatuses", "TenantModuleStatuses", "DataRegions", "InvoiceStatuses", "PaymentMethods", "AddOnRequestStatuses", "ChangeAuditActions", "BranchKinds"];

    internal static void Configure(ModelBuilder builder)
    {
        Map<SecurityAuditEvent, SecurityAuditEventType>(builder, Tables[0], e => e.EventType);
        Map<SecurityAuditEvent, SecurityAuditOutcome>(builder, Tables[1], e => e.Outcome);
        Map<BrandingImage, BrandingImageKind>(builder, Tables[2], i => i.Kind);
        Map<SchoolRequest, SchoolRequestStatus>(builder, Tables[3], r => r.Status);
        Map<State, StateType>(builder, Tables[4], s => s.Type);
        Map<PriceBookItem, BillingCycle>(builder, Tables[5], i => i.BillingCycle);
        Map<Subscription, SubscriptionStatus>(builder, Tables[6], s => s.Status);
        Map<TenantModule, TenantModuleStatus>(builder, Tables[7], m => m.Status);
        Map<School, DataRegion?>(builder, Tables[8], s => s.DataRegion);
        Map<Invoice, InvoiceStatus>(builder, Tables[9], i => i.Status);
        Map<Payment, PaymentMethod>(builder, Tables[10], p => p.Method);
        Map<AddOnRequest, AddOnRequestStatus>(builder, Tables[11], r => r.Status);
        Map<ChangeAuditEntry, ChangeAuditAction>(builder, Tables[12], e => e.Action);
        Map<Branch, BranchKind>(builder, Tables[13], b => b.Kind);

        // More columns of an enum that already has its lookup table.
        Reference<Subscription, BillingCycle>(builder, s => s.BillingCycle);
    }

    // A nullable enum column (TColumn is TEnum?) is allowed to be empty; its lookup table is the one of TEnum.
    private static void Map<TEntity, TColumn>(ModelBuilder builder, string table, Expression<Func<TEntity, TColumn>> column)
        where TEntity : class
    {
        var enumType = Nullable.GetUnderlyingType(typeof(TColumn)) ?? typeof(TColumn);
        typeof(EnumLookupTables)
            .GetMethod(nameof(ConfigureLookup), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
            .MakeGenericMethod(enumType)
            .Invoke(null, [builder, table]);

        Reference<TEntity, TColumn>(builder, column, enumType);
    }

    private static void Reference<TEntity, TColumn>(ModelBuilder builder, Expression<Func<TEntity, TColumn>> column, Type? enumType = null)
        where TEntity : class
    {
        enumType ??= Nullable.GetUnderlyingType(typeof(TColumn)) ?? typeof(TColumn);
        builder.Entity<TEntity>()
            .HasOne(typeof(EnumLookup<>).MakeGenericType(enumType), null)
            .WithMany()
            .HasForeignKey(((MemberExpression)column.Body).Member.Name)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureLookup<TEnum>(ModelBuilder builder, string table)
        where TEnum : struct, Enum
    {
        builder.Entity<EnumLookup<TEnum>>(lookup =>
        {
            lookup.ToTable(table);
            lookup.HasKey(l => l.Id);
            lookup.Property(l => l.Id).ValueGeneratedNever();
            lookup.Property(l => l.Name).IsRequired().HasMaxLength(EnumLookup<TEnum>.NameMaxLength);
            lookup.HasData(Enum.GetValues<TEnum>().Select(value => new EnumLookup<TEnum> { Id = value, Name = value.ToString() }));
        });
    }

}
