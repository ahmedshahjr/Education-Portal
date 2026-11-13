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
        ["SecurityAuditEventTypes", "SecurityAuditOutcomes", "BrandingImageKinds", "SchoolRequestStatuses", "StateTypes"];

    internal static void Configure(ModelBuilder builder)
    {
        Map<SecurityAuditEvent, SecurityAuditEventType>(builder, Tables[0], e => e.EventType);
        Map<SecurityAuditEvent, SecurityAuditOutcome>(builder, Tables[1], e => e.Outcome);
        Map<BrandingImage, BrandingImageKind>(builder, Tables[2], i => i.Kind);
        Map<SchoolRequest, SchoolRequestStatus>(builder, Tables[3], r => r.Status);
        Map<State, StateType>(builder, Tables[4], s => s.Type);
    }

    private static void Map<TEntity, TEnum>(ModelBuilder builder, string table, Expression<Func<TEntity, TEnum>> column)
        where TEntity : class
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

        builder.Entity<TEntity>()
            .HasOne<EnumLookup<TEnum>>()
            .WithMany()
            .HasForeignKey(Convert(column))
            .OnDelete(DeleteBehavior.Restrict);
    }

    // HasForeignKey takes an expression returning object.
    private static Expression<Func<TEntity, object?>> Convert<TEntity, TEnum>(Expression<Func<TEntity, TEnum>> column) =>
        Expression.Lambda<Func<TEntity, object?>>(Expression.Convert(column.Body, typeof(object)), column.Parameters);
}
