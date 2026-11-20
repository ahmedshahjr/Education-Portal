namespace EducationPortal.Domain.Academics;

/// <summary>One timetable slot as the conflict rules see it: when it is, and who, where and which class it is for.</summary>
/// <param name="Id">The slot's id; the slot itself is never a conflict with its own saved version.</param>
public sealed record TimetableSlotSpec(
    int Id,
    DayOfWeek Day,
    TimeOnly Start,
    TimeOnly End,
    int TeacherId,
    int? CampusId,
    string? Room,
    int ClassOfferingId);

/// <summary>What two slots share and clash on.</summary>
public enum TimetableConflictKind
{
    Teacher = 1,

    Room = 2,

    ClassOffering = 3,
}

/// <summary>A slot that clashes with the candidate, and on what.</summary>
public sealed record TimetableConflict(int SlotId, TimetableConflictKind Kind);

/// <summary>
/// The timetable rules (pure, no storage). Two slots clash when they are on the same day and their times overlap, and they
/// share a teacher, a room on the same campus, or a class offering. Touching slots (one ends when the next starts) do not clash.
/// </summary>
public static class TimetableConflicts
{
    /// <summary>The clashes of a candidate slot with the existing slots of the school. Empty when the slot fits.</summary>
    public static IReadOnlyList<TimetableConflict> Find(IEnumerable<TimetableSlotSpec> existing, TimetableSlotSpec candidate)
    {
        ArgumentNullException.ThrowIfNull(existing);
        ArgumentNullException.ThrowIfNull(candidate);
        if (candidate.End <= candidate.Start)
        {
            throw new ArgumentException("A slot ends after it starts.", nameof(candidate));
        }

        var conflicts = new List<TimetableConflict>();
        foreach (var other in existing)
        {
            if (other.Id == candidate.Id || other.Day != candidate.Day || !Overlaps(other, candidate))
            {
                continue;
            }

            if (other.TeacherId == candidate.TeacherId)
            {
                conflicts.Add(new TimetableConflict(other.Id, TimetableConflictKind.Teacher));
            }

            if (SameRoom(other, candidate))
            {
                conflicts.Add(new TimetableConflict(other.Id, TimetableConflictKind.Room));
            }

            if (other.ClassOfferingId == candidate.ClassOfferingId)
            {
                conflicts.Add(new TimetableConflict(other.Id, TimetableConflictKind.ClassOffering));
            }
        }

        return conflicts;
    }

    /// <summary>True when the times share at least one moment. Touching at an end does not count.</summary>
    private static bool Overlaps(TimetableSlotSpec a, TimetableSlotSpec b) => a.Start < b.End && b.Start < a.End;

    private static bool SameRoom(TimetableSlotSpec a, TimetableSlotSpec b) =>
        !string.IsNullOrWhiteSpace(a.Room)
        && !string.IsNullOrWhiteSpace(b.Room)
        && a.CampusId == b.CampusId
        && string.Equals(a.Room.Trim(), b.Room.Trim(), StringComparison.OrdinalIgnoreCase);
}
