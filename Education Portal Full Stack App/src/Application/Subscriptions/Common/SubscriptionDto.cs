namespace EducationPortal.Application.Subscriptions.Common;

/// <summary>One module a school has through its subscription.</summary>
public sealed record TenantModuleDto(string Code, TenantModuleStatus Status, DateTimeOffset ValidFrom, DateTimeOffset? ValidTo);

/// <summary>A school's subscription: what it bought, the price and currency copied when it was made, and its modules.</summary>
/// <param name="Id">The subscription's public id.</param>
/// <param name="PlanCode">The plan bought, or <c>null</c> for add-on modules only.</param>
/// <param name="CountryCode">The country of the price book the price came from.</param>
/// <param name="Amount">The price for one billing cycle in <paramref name="CurrencyCode"/>.</param>
/// <param name="Modules">Core modules, the plan's modules and add-ons, in catalogue order.</param>
public sealed record SubscriptionDto(
    Guid Id,
    string? PlanCode,
    string CountryCode,
    string CurrencyCode,
    BillingCycle BillingCycle,
    decimal Amount,
    SubscriptionStatus Status,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    DateTimeOffset? TrialEndDate,
    IReadOnlyList<TenantModuleDto> Modules);
