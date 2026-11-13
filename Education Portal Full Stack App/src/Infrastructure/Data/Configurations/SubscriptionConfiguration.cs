using EducationPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EducationPortal.Infrastructure.Data.Configurations;

public class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.HasOne<School>().WithMany().HasForeignKey(b => b.SchoolId).OnDelete(DeleteBehavior.Cascade);
        builder.Property(b => b.Name).IsRequired().HasMaxLength(Branch.NameMaxLength);
        builder.HasIndex(b => b.SchoolId);
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

        // A subscription gives a module to the whole school once, or to a branch once.
        builder.HasIndex(m => new { m.SubscriptionId, m.ModuleId, m.BranchId }).IsUnique().AreNullsDistinct(false);
        builder.HasIndex(m => new { m.SchoolId, m.ModuleId });
        builder.HasIndex(m => m.BranchId);
    }
}
