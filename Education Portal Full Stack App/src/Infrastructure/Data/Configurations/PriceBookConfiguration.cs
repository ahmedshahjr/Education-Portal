using EducationPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EducationPortal.Infrastructure.Data.Configurations;

public class PriceBookConfiguration : IEntityTypeConfiguration<PriceBook>
{
    public void Configure(EntityTypeBuilder<PriceBook> builder)
    {
        builder.Property(b => b.CountryCode).IsRequired().HasMaxLength(PriceBook.CountryCodeLength).IsFixedLength();
        builder.Property(b => b.CurrencyCode).IsRequired().HasMaxLength(PriceBook.CurrencyCodeLength).IsFixedLength();

        builder.Property(b => b.TaxRate).HasPrecision(5, 2);
        builder.ToTable(t => t.HasCheckConstraint("CK_PriceBooks_TaxRateRange", "\"TaxRate\" >= 0 AND \"TaxRate\" <= 100"));

        // One live price book per country, so a school's price book follows from its country.
        builder.HasIndex(b => b.CountryCode).IsUnique().HasFilter("\"IsDeleted\" = false");
    }
}

public class PriceBookItemConfiguration : IEntityTypeConfiguration<PriceBookItem>
{
    public void Configure(EntityTypeBuilder<PriceBookItem> builder)
    {
        builder.HasOne<PriceBook>().WithMany().HasForeignKey(i => i.PriceBookId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Plan>().WithMany().HasForeignKey(i => i.PlanId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<PortalModule>().WithMany().HasForeignKey(i => i.ModuleId).OnDelete(DeleteBehavior.Restrict);

        // Money is decimal, never floating point; the currency is the price book's (CurrencyCode).
        builder.Property(i => i.Price).HasPrecision(18, PriceBookItem.PriceScale);

        // One price per plan or module and billing cycle in a book.
        builder.HasIndex(i => new { i.PriceBookId, i.PlanId, i.BillingCycle }).IsUnique().HasFilter("\"PlanId\" IS NOT NULL");
        builder.HasIndex(i => new { i.PriceBookId, i.ModuleId, i.BillingCycle }).IsUnique().HasFilter("\"ModuleId\" IS NOT NULL");
        builder.HasIndex(i => i.PlanId);
        builder.HasIndex(i => i.ModuleId);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_PriceBookItems_PlanOrModule", "(\"PlanId\" IS NULL) <> (\"ModuleId\" IS NULL)");
            t.HasCheckConstraint("CK_PriceBookItems_PriceNotNegative", "\"Price\" >= 0");
        });
    }
}
