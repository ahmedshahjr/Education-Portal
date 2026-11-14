using EducationPortal.Domain.Common;

namespace EducationPortal.Domain.Entities;

/// <summary>
/// A named daily shift of a campus, such as Morning or Evening, with its start and end time. The times are local times
/// of the school's country. A shift ends on the same day it starts (an overnight shift is refused). Replaced as a set by
/// <see cref="Branch.ReplaceShifts"/>; a replaced shift is soft-deleted and kept for audit.
/// </summary>
public class CampusShift : BaseSoftDeletableEntity
{
    public const int NameMaxLength = 50;

    private CampusShift()
    {
        // For EF Core.
        Name = string.Empty;
    }

    private CampusShift(int schoolId, int branchId, string name, TimeOnly startTime, TimeOnly endTime)
    {
        SchoolId = schoolId;
        BranchId = branchId;
        Name = name;
        StartTime = startTime;
        EndTime = endTime;
    }

    /// <summary>Creates a shift. The name is trimmed; the end must be after the start.</summary>
    public static CampusShift Create(int schoolId, int branchId, string name, TimeOnly startTime, TimeOnly endTime)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(schoolId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(branchId);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(name.Trim().Length, NameMaxLength, nameof(name));

        if (endTime <= startTime)
        {
            throw new ArgumentException("A shift ends after it starts on the same day.", nameof(endTime));
        }

        return new CampusShift(schoolId, branchId, name.Trim(), startTime, endTime);
    }

    public int SchoolId { get; private set; }

    public int BranchId { get; private set; }

    public string Name { get; private set; }

    public TimeOnly StartTime { get; private set; }

    public TimeOnly EndTime { get; private set; }
}
