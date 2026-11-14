using EducationPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EducationPortal.Infrastructure.Data.Configurations;

public class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.HasOne<School>().WithMany(s => s.Branches).HasForeignKey(b => b.SchoolId).OnDelete(DeleteBehavior.Cascade);
        builder.Property(b => b.Name).IsRequired().HasMaxLength(Branch.NameMaxLength);
        builder.Property(b => b.Code).IsRequired().HasMaxLength(Branch.CodeLength);

        // Codes are unique within a school. This index also serves the SchoolId foreign key (ENG-EF-09).
        // Deleted branches keep their rows but free their code.
        builder.HasIndex(b => new { b.SchoolId, b.Code }).IsUnique().HasFilter("\"IsDeleted\" = false");

        // Every school has exactly one main branch, its registration branch (the Kind value 1 is BranchKind.Main).
        builder.HasIndex(b => b.SchoolId)
            .IsUnique()
            .HasFilter("\"Kind\" = 1")
            .HasDatabaseName("IX_Branches_SingleMainPerSchool");
    }
}

public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.HasOne<School>().WithMany().HasForeignKey(s => s.SchoolId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Plan>().WithMany().HasForeignKey(s => s.PlanId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<PriceBook>().WithMany().HasForeignKey(s => s.PriceBookId).OnDelete(DeleteBehavior.Restrict);

        builder.Property(s => s.CurrencyCode).IsRequired().HasMaxLength(PriceBook.CurrencyCodeLength).IsFixedLength();

        // Money is decimal, never floating point, with the currency next to it.
        builder.Property(s => s.Amount).HasPrecision(18, PriceBookItem.PriceScale);

        // A school has at most one subscription that is not cancelled (Cancelled = 5).
        builder.HasIndex(s => s.SchoolId).IsUnique().HasFilter("\"Status\" <> 5");
        builder.HasIndex(s => s.PlanId);
        builder.HasIndex(s => s.PriceBookId);

        builder.ToTable(t => t.HasCheckConstraint("CK_Subscriptions_AmountNotNegative", "\"Amount\" >= 0"));

        builder.HasMany(s => s.Modules).WithOne(m => m.Subscription).HasForeignKey(m => m.SubscriptionId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(s => s.Modules).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public class TenantModuleConfiguration : IEntityTypeConfiguration<TenantModule>
{
    public void Configure(EntityTypeBuilder<TenantModule> builder)
    {
        builder.HasOne<School>().WithMany().HasForeignKey(m => m.SchoolId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<PortalModule>().WithMany().HasForeignKey(m => m.ModuleId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Branch>().WithMany().HasForeignKey(m => m.BranchId).OnDelete(DeleteBehavior.Restrict);

        // A subscription gives a module to each branch once. The main branch's grant is the whole school's.
        builder.HasIndex(m => new { m.SubscriptionId, m.ModuleId, m.BranchId }).IsUnique();
        builder.HasIndex(m => new { m.SchoolId, m.ModuleId });
        builder.HasIndex(m => m.BranchId);
    }
}

public class AddOnRequestConfiguration : IEntityTypeConfiguration<AddOnRequest>
{
    public void Configure(EntityTypeBuilder<AddOnRequest> builder)
    {
        builder.HasOne<School>().WithMany().HasForeignKey(r => r.SchoolId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<PortalModule>().WithMany().HasForeignKey(r => r.ModuleId).OnDelete(DeleteBehavior.Restrict);

        // A school has at most one pending request per module (Pending = 1).
        builder.HasIndex(r => new { r.SchoolId, r.ModuleId }).IsUnique().HasFilter("\"Status\" = 1");
        builder.HasIndex(r => r.ModuleId);
    }
}

public class RenewalReminderConfiguration : IEntityTypeConfiguration<RenewalReminder>
{
    public void Configure(EntityTypeBuilder<RenewalReminder> builder)
    {
        builder.HasOne<School>().WithMany().HasForeignKey(r => r.SchoolId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Subscription>().WithMany().HasForeignKey(r => r.SubscriptionId).OnDelete(DeleteBehavior.Cascade);

        // A reminder is queued once per subscription, period and reminder day, even if the job runs twice.
        builder.HasIndex(r => new { r.SubscriptionId, r.PeriodEnd, r.DaysBefore }).IsUnique();
        builder.HasIndex(r => r.SchoolId);

        // The queue a sender reads: reminders not sent yet.
        builder.HasIndex(r => r.QueuedAt).HasFilter("\"SentAt\" IS NULL").HasDatabaseName("IX_RenewalReminders_Unsent");

        builder.ToTable(t => t.HasCheckConstraint("CK_RenewalReminders_DaysBefore", $"\"DaysBefore\" BETWEEN 1 AND {RenewalReminder.MaxDaysBefore}"));
    }
}
