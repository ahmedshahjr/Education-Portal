using EducationPortal.Domain.Common;

namespace EducationPortal.Domain.Entities;

/// <summary>
/// A shift a campus runs, for example <c>Morning</c> from 07:30 to 13:00. Campus data: it has the school and the campus
/// (SEC-AUTHZ-03). A shift ends on the day it starts, so the start is before the end. Names are unique within a campus.
/// </summary>
public class Shift : BaseSoftDeletableEntity, ITenantOwned
{
    public const int NameMaxLength = 50;

    private Shift()
    {
        // For EF Core.
        Name = string.Empty;
    }

    public static Shift Create(int schoolId, int campusId, string name, TimeOnly startTime, TimeOnly endTime)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(schoolId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(campusId);

        var shift = new Shift { SchoolId = schoolId, CampusId = campusId };
        shift.Change(name, startTime, endTime);
        return shift;
    }

    public int SchoolId { get; private set; }

    public int CampusId { get; private set; }

    public string Name { get; private set; }

    public TimeOnly StartTime { get; private set; }

    public TimeOnly EndTime { get; private set; }

    public void Change(string name, TimeOnly startTime, TimeOnly endTime)
    {
        if (IsDeleted)
        {
            throw new InvalidOperationException("A deleted shift cannot be changed.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(name.Trim().Length, NameMaxLength, nameof(name));
        if (endTime <= startTime)
        {
            throw new ArgumentException("A shift ends after it starts, on the same day.", nameof(endTime));
        }

        Name = name.Trim();
        StartTime = startTime;
        EndTime = endTime;
    }
}
