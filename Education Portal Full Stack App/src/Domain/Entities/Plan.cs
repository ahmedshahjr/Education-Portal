using System.Text.RegularExpressions;

namespace EducationPortal.Domain.Entities;

/// <summary>
/// A bundle of paid modules the master tenant sells, such as Starter or Full. Core modules are in every subscription,
/// so a plan lists only paid modules (<see cref="PlanModule"/>). Platform reference data that only the master tenant
/// changes, so it has no <c>SchoolId</c>. Prices live in price books (<see cref="PriceBookItem"/>), per country.
/// </summary>
public partial class Plan : BaseSoftDeletableEntity
{
    public const int CodeMaxLength = 50;
    public const int NameMaxLength = 100;
    public const int DescriptionMaxLength = 500;

    private Plan()
    {
        // For EF Core.
        Code = string.Empty;
        Name = string.Empty;
        Description = string.Empty;
    }

    private Plan(string code) : this()
    {
        if (!IsValidCode(code))
        {
            throw new ArgumentException("A plan code is upper-case letters, digits and underscores, starting with a letter, for example STARTER.", nameof(code));
        }

        Code = code;
    }

    public static Plan Create(string code, string name, string description)
    {
        var plan = new Plan(code);
        plan.Update(name, description);
        return plan;
    }

    /// <summary>The stable code, for example <c>STARTER</c>. Set once.</summary>
    public string Code { get; private set; }

    public string Name { get; private set; }

    public string Description { get; private set; }

    public void Update(string name, string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(name.Trim().Length, NameMaxLength, nameof(name));
        ArgumentNullException.ThrowIfNull(description);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(description.Trim().Length, DescriptionMaxLength, nameof(description));

        Name = name.Trim();
        Description = description.Trim();
    }

    /// <summary>True for upper-case letters, digits and underscores that start with a letter and fit the column.</summary>
    public static bool IsValidCode(string? code) =>
        code is not null && code.Length <= CodeMaxLength && CodePattern().IsMatch(code);

    [GeneratedRegex("^[A-Z][A-Z0-9_]*$")]
    private static partial Regex CodePattern();
}
