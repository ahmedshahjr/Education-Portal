using EducationPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EducationPortal.Infrastructure.Data.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public const string NumberSequence = "InvoiceNumbers";

    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.HasOne<School>().WithMany().HasForeignKey(i => i.SchoolId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Subscription>().WithMany().HasForeignKey(i => i.SubscriptionId).OnDelete(DeleteBehavior.Restrict);

        // The number comes from a sequence when the row is inserted: unique, readable, not gap-free.
        builder.Property(i => i.Number)
            .HasMaxLength(Invoice.NumberMaxLength)
            .HasDefaultValueSql($"'INV-' || lpad(nextval('\"{NumberSequence}\"')::text, 6, '0')")
            .HasSentinel(string.Empty)
            .ValueGeneratedOnAdd();
        builder.HasIndex(i => i.Number).IsUnique();

        builder.Property(i => i.CurrencyCode).IsRequired().HasMaxLength(PriceBook.CurrencyCodeLength).IsFixedLength();

        // Money is decimal, never floating point, with the currency next to it.
        builder.Property(i => i.Subtotal).HasPrecision(18, PriceBookItem.PriceScale);
        builder.Property(i => i.TaxAmount).HasPrecision(18, PriceBookItem.PriceScale);
        builder.Property(i => i.Total).HasPrecision(18, PriceBookItem.PriceScale);
        builder.Property(i => i.TaxRate).HasPrecision(5, 2);

        // One open invoice per subscription (Open = 1), and a period is never invoiced twice unless the first was voided (Void = 3).
        builder.HasIndex(i => i.SubscriptionId).IsUnique().HasFilter("\"Status\" = 1").HasDatabaseName("IX_Invoices_OneOpenPerSubscription");
        builder.HasIndex(i => new { i.SubscriptionId, i.PeriodStart }).IsUnique().HasFilter("\"Status\" <> 3").HasDatabaseName("IX_Invoices_PeriodInvoicedOnce");
        builder.HasIndex(i => i.SchoolId);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_Invoices_AmountsNotNegative", "\"Subtotal\" >= 0 AND \"TaxAmount\" >= 0 AND \"Total\" >= 0");
            t.HasCheckConstraint("CK_Invoices_PeriodOrder", "\"PeriodEnd\" > \"PeriodStart\"");
        });

        builder.HasMany(i => i.Lines).WithOne(l => l.Invoice).HasForeignKey(l => l.InvoiceId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(i => i.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.HasMany(i => i.Payments).WithOne(p => p.Invoice).HasForeignKey(p => p.InvoiceId).OnDelete(DeleteBehavior.Restrict);
        builder.Navigation(i => i.Payments).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public class InvoiceLineConfiguration : IEntityTypeConfiguration<InvoiceLine>
{
    public void Configure(EntityTypeBuilder<InvoiceLine> builder)
    {
        builder.HasOne<School>().WithMany().HasForeignKey(l => l.SchoolId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(l => l.Description).IsRequired().HasMaxLength(InvoiceLine.DescriptionMaxLength);
        builder.Property(l => l.Amount).HasPrecision(18, PriceBookItem.PriceScale);
        builder.HasIndex(l => l.InvoiceId);
    }
}

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasOne<School>().WithMany().HasForeignKey(p => p.SchoolId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(p => p.CurrencyCode).IsRequired().HasMaxLength(PriceBook.CurrencyCodeLength).IsFixedLength();
        builder.Property(p => p.Amount).HasPrecision(18, PriceBookItem.PriceScale);
        builder.Property(p => p.Reference).IsRequired().HasMaxLength(Payment.ReferenceMaxLength);
        builder.Property(p => p.Provider).IsRequired().HasMaxLength(Payment.ProviderMaxLength);
        builder.HasIndex(p => p.InvoiceId);
        builder.HasIndex(p => p.SchoolId);
        builder.ToTable(t => t.HasCheckConstraint("CK_Payments_AmountPositive", "\"Amount\" > 0"));
    }
}
