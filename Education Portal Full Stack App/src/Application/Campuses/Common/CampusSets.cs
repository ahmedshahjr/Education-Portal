namespace EducationPortal.Application.Campuses.Common;

/// <summary>Set arithmetic on allowed campus ids, where <c>null</c> stands for every campus of the school (ADR-022).</summary>
public static class CampusSets
{
    /// <summary>
    /// The campuses in both sets. <c>null</c> (every campus) on one side gives the other side; <c>null</c> on both gives
    /// <c>null</c>. An empty side gives nothing.
    /// </summary>
    public static IReadOnlyCollection<int>? Intersect(IReadOnlyCollection<int>? first, IReadOnlyCollection<int>? second)
    {
        if (first is null)
        {
            return second;
        }

        if (second is null)
        {
            return first;
        }

        return first.Intersect(second).ToList();
    }
}
