using EducationPortal.Domain.Entities;

namespace EducationPortal.Domain.UnitTests;

/// <summary>The campuses the domain tests grant modules on. Every test subscription belongs to school 9.</summary>
internal static class TestCampuses
{
    public const int SchoolId = 9;

    /// <summary>School 9's main campus, as if saved (id 1). Every school has one, created with it.</summary>
    public static Campus MainCampus()
    {
        var campus = School.Create("School nine", "school9.example.com").Campuses.Single();
        typeof(Campus).GetProperty(nameof(Campus.SchoolId))!.SetValue(campus, SchoolId);
        campus.Id = 1;
        return campus;
    }
}
