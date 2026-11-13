namespace EducationPortal.Domain.Enums;

/// <summary>
/// How often a price is charged. Stored as its number and sent over the API by name (ENG-ENUM-01..03): never change or
/// reuse a number, and never rename a member.
/// </summary>
public enum BillingCycle
{
    Monthly = 1,
    Yearly = 2,
}
