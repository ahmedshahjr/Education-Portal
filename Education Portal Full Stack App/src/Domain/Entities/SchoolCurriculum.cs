using EducationPortal.Domain.Common;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Domain.Entities;

/// <summary>
/// A board or curriculum a school teaches (the table <c>SchoolCurricula</c>), shown on its public page. A school lists
/// each kind at most once.
/// </summary>
public class SchoolCurriculum : BaseAuditableEntity, ITenantOwned
{
    private SchoolCurriculum()
    {
        // For EF Core.
    }

    public static SchoolCurriculum Create(int schoolId, CurriculumKind kind)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(schoolId);

        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(nameof(kind));
        }

        return new SchoolCurriculum { SchoolId = schoolId, Kind = kind };
    }

    public int SchoolId { get; private set; }

    public CurriculumKind Kind { get; private set; }
}
