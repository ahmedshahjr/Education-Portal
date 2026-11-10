using System.Text.RegularExpressions;

namespace EducationPortal.Domain.ValueObjects;

/// <summary>
/// A colour in <c>#rrggbb</c> form, for example a school's sidebar colour. Only this exact form is accepted, so the
/// value can be put into a CSS custom property without any risk of injecting other CSS (SEC-INPUT-01).
/// </summary>
public sealed partial class HexColor : ValueObject
{
    public const int Length = 7;

    private HexColor(string value)
    {
        Value = value;
    }

    /// <summary>The colour in lower case, for example <c>#1f2a3d</c>.</summary>
    public string Value { get; }

    public static bool IsValid(string? value) => value is not null && Pattern().IsMatch(value);

    public static HexColor From(string value)
    {
        if (!IsValid(value))
        {
            throw new ArgumentException("A colour must have the form #rrggbb.", nameof(value));
        }

        return new HexColor(value.ToLowerInvariant());
    }

    public override string ToString() => Value;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    [GeneratedRegex("^#[0-9A-Fa-f]{6}$")]
    private static partial Regex Pattern();
}
