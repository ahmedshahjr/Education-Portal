namespace EducationPortal.Domain.Enums;

/// <summary>Where an invoice stands. Whether an open invoice is overdue follows from its due date; the lifecycle job (phase 7) acts on that.</summary>
public enum InvoiceStatus
{
    /// <summary>Issued and not paid yet.</summary>
    Open = 1,

    /// <summary>Paid in full.</summary>
    Paid = 2,

    /// <summary>Cancelled by the master tenant before payment. Kept for audit.</summary>
    Void = 3,
}
