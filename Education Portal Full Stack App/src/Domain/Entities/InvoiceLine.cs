namespace EducationPortal.Domain.Entities;

/// <summary>One line of an invoice: what is billed and how much (before tax), in the invoice's currency.</summary>
public class InvoiceLine : BaseEntity
{
    public const int DescriptionMaxLength = 200;

    private InvoiceLine()
    {
        // For EF Core.
        Description = string.Empty;
    }

    internal static InvoiceLine Create(Invoice invoice, string description, decimal amount)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(description.Trim().Length, DescriptionMaxLength, nameof(description));

        if (!PriceBookItem.IsValidPrice(amount))
        {
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "An amount is zero or more, with at most two decimal places.");
        }

        return new InvoiceLine { Invoice = invoice, SchoolId = invoice.SchoolId, Description = description.Trim(), Amount = amount };
    }

    public int SchoolId { get; private set; }

    public int InvoiceId { get; private set; }

    public Invoice? Invoice { get; private set; }

    public string Description { get; private set; }

    public decimal Amount { get; private set; }
}
