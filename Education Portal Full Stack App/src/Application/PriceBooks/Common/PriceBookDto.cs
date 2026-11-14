namespace EducationPortal.Application.PriceBooks.Common;

/// <summary>One price: for a plan (<see cref="PlanCode"/>) or a paid module (<see cref="ModuleCode"/>), never both.</summary>
public record PriceDto(string? PlanCode, string? ModuleCode, BillingCycle BillingCycle, decimal Price);

/// <summary>A country's price book: its currency, its tax rate in percent (added to invoices) and every price in it, in that currency.</summary>
public record PriceBookDto(Guid Id, string CountryCode, string CurrencyCode, decimal TaxRate, bool IsActive, IReadOnlyList<PriceDto> Prices);
