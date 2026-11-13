using System.Text.RegularExpressions;

namespace EducationPortal.Domain.Entities;

/// <summary>
/// The prices for one country, in one currency, for example Pakistan in PKR. A school is priced from the price book of
/// its country. A price book prices plans and paid add-on modules per billing cycle (<see cref="PriceBookItem"/>), all in
/// the book's currency. Platform reference data that only the master tenant changes, so it has no <c>SchoolId</c>.
/// </summary>
public partial class PriceBook : BaseSoftDeletableEntity
{
    public const int CountryCodeLength = 2;
    public const int CurrencyCodeLength = 3;

    private PriceBook()
    {
        // For EF Core.
        CountryCode = string.Empty;
        CurrencyCode = string.Empty;
    }

    private PriceBook(string countryCode) : this()
    {
        if (!Country.IsValidCode(countryCode))
        {
            throw new ArgumentException("A country code is two letters (ISO 3166-1 alpha-2), for example PK.", nameof(countryCode));
        }

        CountryCode = countryCode.ToUpperInvariant();
    }

    public static PriceBook Create(string countryCode, string currencyCode)
    {
        var book = new PriceBook(countryCode);
        book.SetCurrency(currencyCode);
        return book;
    }

    /// <summary>The ISO 3166-1 alpha-2 code of the country it prices, in upper case, for example <c>PK</c>. Set once.</summary>
    public string CountryCode { get; private set; }

    /// <summary>The ISO 4217 code every price in the book is in, for example <c>PKR</c>.</summary>
    public string CurrencyCode { get; private set; }

    /// <summary>The tax (VAT/GST/sales tax) the country adds to an invoice, as a percentage from 0 to 100 with at most two decimals.</summary>
    public decimal TaxRate { get; private set; }

    public void SetTaxRate(decimal taxRate)
    {
        if (!IsValidTaxRate(taxRate))
        {
            throw new ArgumentOutOfRangeException(nameof(taxRate), taxRate, "A tax rate is a percentage from 0 to 100 with at most two decimal places.");
        }

        TaxRate = taxRate;
    }

    public static bool IsValidTaxRate(decimal taxRate) => taxRate is >= 0 and <= 100 && decimal.Round(taxRate, 2) == taxRate;

    /// <summary>
    /// Changes the currency. The prices are numbers in the book's currency, so changing it changes what every price
    /// means: the master tenant re-enters the prices with it.
    /// </summary>
    public void SetCurrency(string currencyCode)
    {
        if (!IsValidCurrencyCode(currencyCode))
        {
            throw new ArgumentException("A currency code is three upper-case letters (ISO 4217), for example PKR.", nameof(currencyCode));
        }

        CurrencyCode = currencyCode;
    }

    public static bool IsValidCurrencyCode(string? currencyCode) => currencyCode is not null && CurrencyPattern().IsMatch(currencyCode);

    [GeneratedRegex("^[A-Z]{3}$")]
    private static partial Regex CurrencyPattern();
}
