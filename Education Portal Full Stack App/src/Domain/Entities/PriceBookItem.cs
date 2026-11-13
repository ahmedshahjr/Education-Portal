namespace EducationPortal.Domain.Entities;

/// <summary>
/// One price in a price book: either a plan or a paid add-on module, for one billing cycle, in the book's currency.
/// Exactly one of <see cref="PlanId"/> and <see cref="ModuleId"/> is set. Core modules are never priced on their own.
/// </summary>
public class PriceBookItem : BaseAuditableEntity
{
    /// <summary>Prices have at most two decimal places, which every currency the platform sells in uses.</summary>
    public const int PriceScale = 2;

    /// <summary>The largest price the column holds (numeric(18,2)).</summary>
    public const decimal MaxPrice = 9_999_999_999_999_999.99m;

    private PriceBookItem()
    {
        // For EF Core.
    }

    private PriceBookItem(int priceBookId, int? planId, int? moduleId, BillingCycle billingCycle, decimal price)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(priceBookId);

        if (planId is null == moduleId is null)
        {
            throw new ArgumentException("A price is for either a plan or a module, not both and not neither.");
        }

        if (!Enum.IsDefined(billingCycle))
        {
            throw new ArgumentOutOfRangeException(nameof(billingCycle), billingCycle, "Unknown billing cycle.");
        }

        PriceBookId = priceBookId;
        PlanId = planId;
        ModuleId = moduleId;
        BillingCycle = billingCycle;
        SetPrice(price);
    }

    public static PriceBookItem ForPlan(PriceBook book, Plan plan, BillingCycle billingCycle, decimal price)
    {
        ArgumentNullException.ThrowIfNull(book);
        ArgumentNullException.ThrowIfNull(plan);

        return new PriceBookItem(book.Id, plan.Id, null, billingCycle, price);
    }

    public static PriceBookItem ForModule(PriceBook book, PortalModule module, BillingCycle billingCycle, decimal price)
    {
        ArgumentNullException.ThrowIfNull(book);
        ArgumentNullException.ThrowIfNull(module);

        if (module.IsCore)
        {
            throw new ArgumentException("A core module is in every subscription and is never sold on its own.", nameof(module));
        }

        return new PriceBookItem(book.Id, null, module.Id, billingCycle, price);
    }

    public int PriceBookId { get; private set; }

    public int? PlanId { get; private set; }

    public int? ModuleId { get; private set; }

    public BillingCycle BillingCycle { get; private set; }

    /// <summary>The price per billing cycle, in the price book's currency.</summary>
    public decimal Price { get; private set; }

    public void SetPrice(decimal price)
    {
        if (!IsValidPrice(price))
        {
            throw new ArgumentOutOfRangeException(nameof(price), price, "A price is zero or more, with at most two decimal places.");
        }

        Price = price;
    }

    /// <summary>True for a price of zero or more with at most two decimal places that fits the column.</summary>
    public static bool IsValidPrice(decimal price) =>
        price >= 0 && price <= MaxPrice && decimal.Round(price, PriceScale) == price;
}
