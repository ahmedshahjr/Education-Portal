using EducationPortal.Domain.Common;
using EducationPortal.Domain.ValueObjects;

namespace EducationPortal.Domain.Entities;

/// <summary>
/// One phone number of a campus (the table <c>CampusPhones</c>). A campus has one to <see cref="Campus.MaxPhones"/> numbers,
/// in the order the school gave them. Owned by the school (<see cref="SchoolId"/>, filled from the campus) and covered by the
/// tenant query filter (SEC-AUTHZ-03).
/// </summary>
public class CampusPhone : BaseEntity
{
    private CampusPhone()
    {
        // For EF Core.
        Number = string.Empty;
    }

    internal CampusPhone(Campus campus, string number, int position)
    {
        ArgumentNullException.ThrowIfNull(campus);
        SchoolId = campus.SchoolId;
        Campus = campus;
        Number = number;
        Position = position;
    }

    public int SchoolId { get; private set; }

    public int CampusId { get; private set; }

    public Campus? Campus { get; private set; }

    /// <summary>The number, as the school wrote it (trimmed).</summary>
    public string Number { get; private set; }

    /// <summary>The place of the number in the campus's list, from 0.</summary>
    public int Position { get; private set; }

    /// <summary>True when the number has the form <see cref="ContactRules.IsValidPhone"/> accepts.</summary>
    public static bool IsValid(string number) => ContactRules.IsValidPhone(number.Trim());
}
