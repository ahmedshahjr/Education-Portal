using EducationPortal.Application.Campuses.Common;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Application.UnitTests.Campuses;

/// <summary>R1.4 (ADR-022): a user's allowed campuses are the user's assignments intersected with the campuses their roles grant; <c>null</c> means every campus.</summary>
public class CampusSetsTests
{
    [Test]
    public void ShouldReturnTheOtherSetWhenOneIsEveryCampus()
    {
        CampusSets.Intersect(null, new[] { 1, 2 }).ShouldNotBeNull().OrderBy(i => i).ShouldBe(new[] { 1, 2 });
        CampusSets.Intersect(new[] { 3 }, null).ShouldNotBeNull().ShouldBe(new[] { 3 });
    }

    [Test]
    public void ShouldReturnNullOnlyWhenBothAreEveryCampus() =>
        CampusSets.Intersect(null, null).ShouldBeNull();

    [Test]
    public void ShouldKeepOnlyTheCampusesInBothSets() =>
        CampusSets.Intersect(new[] { 1, 2, 3 }, new[] { 2, 3, 4 }).ShouldNotBeNull().OrderBy(i => i).ShouldBe(new[] { 2, 3 });

    [Test]
    public void ShouldReturnNothingWhenTheSetsDoNotOverlap() =>
        CampusSets.Intersect(new[] { 1 }, new[] { 2 }).ShouldNotBeNull().ShouldBeEmpty();

    [Test]
    public void ShouldReturnNothingWhenEitherSetIsEmpty()
    {
        CampusSets.Intersect(Array.Empty<int>(), new[] { 1 }).ShouldNotBeNull().ShouldBeEmpty();
        CampusSets.Intersect(null, Array.Empty<int>()).ShouldNotBeNull().ShouldBeEmpty();
    }
}
