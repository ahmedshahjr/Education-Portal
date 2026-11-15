using System.Text.RegularExpressions;

namespace EducationPortal.Domain.ValueObjects;

/// <summary>
/// The format rules for contact details of a campus and of a person. Shared by the entities and the validators, so the
/// same rule applies wherever a value is entered (SEC-INPUT-01).
/// </summary>
public static partial class ContactRules
{
    public const int PhoneMaxLength = 30;
    public const int PhoneMinDigits = 7;
    public const int PhoneMaxDigits = 15;
    public const int PostalCodeMaxLength = 10;
    public const int WebsiteMaxLength = 2048;

    /// <summary>An optional leading +, then digits, spaces, hyphens and brackets. Between 7 and 15 digits (E.164).</summary>
    public static bool IsValidPhone(string value) =>
        value.Length <= PhoneMaxLength
        && PhonePattern().IsMatch(value)
        && value.Count(char.IsAsciiDigit) is >= PhoneMinDigits and <= PhoneMaxDigits;

    /// <summary>Letters, digits, spaces and hyphens, up to 10 characters, starting with a letter or digit.</summary>
    public static bool IsValidPostalCode(string value) =>
        value.Length <= PostalCodeMaxLength && PostalCodePattern().IsMatch(value);

    /// <summary>An absolute http or https address, up to 2048 characters.</summary>
    public static bool IsValidWebsite(string value) =>
        value.Length <= WebsiteMaxLength
        && Uri.TryCreate(value, UriKind.Absolute, out var uri)
        && uri.Scheme is "http" or "https";

    [GeneratedRegex(@"^\+?[0-9][0-9 ()\-]*$")]
    private static partial Regex PhonePattern();

    [GeneratedRegex(@"^[A-Za-z0-9][A-Za-z0-9 \-]*$")]
    private static partial Regex PostalCodePattern();
}
