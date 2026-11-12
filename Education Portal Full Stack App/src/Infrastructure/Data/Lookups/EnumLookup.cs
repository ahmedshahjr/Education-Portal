namespace EducationPortal.Infrastructure.Data.Lookups;

/// <summary>
/// One row of a lookup table that names the numbers an enum is stored as (ENG-ENUM-04). Enum columns hold numbers, so
/// reports and SQL reviews (for example of the security audit log) join this table to read the names. The rows are
/// seeded from the enum itself, so the table always matches the code, and a foreign key to it stops a number that the
/// enum does not define from being stored. Infrastructure only: the application keeps using the enum.
/// </summary>
public sealed class EnumLookup<TEnum>
    where TEnum : struct, Enum
{
    public const int NameMaxLength = 64;

    public required TEnum Id { get; init; }

    public required string Name { get; init; }
}
