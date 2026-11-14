using EducationPortal.Domain.Entities;

namespace EducationPortal.Domain.UnitTests;

/// <summary>The branches the domain tests grant modules on. Every test subscription belongs to school 9.</summary>
internal static class TestBranches
{
    public const int SchoolId = 9;

    /// <summary>School 9's main branch, as if saved (id 1). Every school has one, created with it.</summary>
    public static Branch MainBranch()
    {
        var branch = School.Create("School nine", "school9.example.com").Branches.Single();
        typeof(Branch).GetProperty(nameof(Branch.SchoolId))!.SetValue(branch, SchoolId);
        branch.Id = 1;
        return branch;
    }
}
