using System.Globalization;
using EducationPortal.Domain.Common;
using EducationPortal.Domain.Enums;
using EducationPortal.Infrastructure.Data.Interceptors;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace EducationPortal.Infrastructure.IntegrationTests.Data;

/// <summary>
/// What a change audit entry records (ADR-016): culture-free values, every value of an added or removed row, only the
/// changed values of a modified row, and never the key or the audit fields the auditable-entity interceptor keeps.
/// The database end to end is covered by the functional ChangeAuditTests.
/// </summary>
public class ChangeAuditInterceptorTests
{
    [Test]
    public void ShouldFormatValuesTheSameInEveryCulture()
    {
        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ar-SA");

            ChangeAuditInterceptor.Format(null).ShouldBeNull();
            ChangeAuditInterceptor.Format(1234.50m).ShouldBe("1234.5");
            ChangeAuditInterceptor.Format(25000.00m).ShouldBe("25000");
            ChangeAuditInterceptor.Format(0m).ShouldBe("0");
            ChangeAuditInterceptor.Format(42).ShouldBe("42");
            ChangeAuditInterceptor.Format(SubscriptionStatus.PastDue).ShouldBe("PastDue");
            ChangeAuditInterceptor.Format("PKR").ShouldBe("PKR");
            ChangeAuditInterceptor.Format(true).ShouldBe("True");
            ChangeAuditInterceptor.Format(new DateTimeOffset(2026, 11, 13, 10, 0, 0, TimeSpan.FromHours(5)))
                .ShouldBe("2026-11-13T05:00:00.0000000+00:00"); // stored in UTC
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    [Test]
    public void ShouldRecordEveryValueOfAnAddedRowButNotTheKeyOrAuditFields()
    {
        using var context = new SampleContext();
        var row = new Sample { Status = SubscriptionStatus.Active, Amount = 10m, Note = null };

        var changes = ChangeAuditInterceptor.Describe(context.Add(row));

        changes.Select(c => c.Property).ShouldBe([nameof(Sample.Amount), nameof(Sample.Note), nameof(BaseEntity.PublicId), nameof(Sample.Status)], ignoreOrder: true);
        changes.ShouldAllBe(c => c.Old == null);
        changes.Single(c => c.Property == nameof(Sample.Status)).New.ShouldBe("Active");
    }

    [Test]
    public void ShouldRecordOnlyTheChangedValuesOfAModifiedRow()
    {
        using var context = new SampleContext();
        var row = new Sample { Id = 7, Status = SubscriptionStatus.Active, Amount = 10m };
        var entry = context.Attach(row);

        row.Status = SubscriptionStatus.Suspended;
        row.LastModified = DateTimeOffset.UtcNow; // kept by the auditable-entity interceptor, not repeated here
        context.ChangeTracker.DetectChanges();

        ChangeAuditInterceptor.Describe(entry).ShouldHaveSingleItem()
            .ShouldBe(new ChangeAuditInterceptor.PropertyChange(nameof(Sample.Status), "Active", "Suspended"));
    }

    [Test]
    public void ShouldRecordNothingWhenAValueIsSetToWhatItAlreadyWas()
    {
        using var context = new SampleContext();
        var row = new Sample { Id = 7, Status = SubscriptionStatus.Active, Amount = 10m };
        var entry = context.Attach(row);

        entry.Property(r => r.Amount).CurrentValue = 10m;
        entry.Property(r => r.Amount).IsModified = true;

        ChangeAuditInterceptor.Describe(entry).ShouldBeEmpty();
    }

    [Test]
    public void ShouldRecordEveryOldValueOfARemovedRow()
    {
        using var context = new SampleContext();
        var row = new Sample { Id = 7, Status = SubscriptionStatus.Active, Amount = 10m, Note = "x" };
        var entry = context.Attach(row);

        context.Remove(row);

        var changes = ChangeAuditInterceptor.Describe(entry);
        changes.ShouldAllBe(c => c.New == null);
        changes.Single(c => c.Property == nameof(Sample.Note)).Old.ShouldBe("x");
    }

    private sealed class Sample : BaseAuditableEntity
    {
        public SubscriptionStatus Status { get; set; }

        public decimal Amount { get; set; }

        public string? Note { get; set; }
    }

    /// <summary>Only tracks changes; it never opens a connection.</summary>
    private sealed class SampleContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
            optionsBuilder.UseNpgsql("Host=unused");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Sample>().Ignore(s => s.DomainEvents);
        }
    }
}
