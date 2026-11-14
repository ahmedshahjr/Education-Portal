using System.Text.RegularExpressions;
using EducationPortal.Domain.Entities;
using EducationPortal.Infrastructure.Data;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Infrastructure.IntegrationTests.Data;

/// <summary>The development sample schools must follow the tenant rules: valid host names, unique domains, and names that fit.</summary>
public class RealSchoolCatalogTests
{
    private static readonly Regex SlugPattern = new("^[a-z0-9]+(-[a-z0-9]+)*$", RegexOptions.CultureInvariant);

    [Test]
    public void ShouldHaveAtLeastTenSchools() =>
        RealSchoolCatalog.Schools.Count.ShouldBeGreaterThanOrEqualTo(10);

    [Test]
    public void EveryDomainShouldBeAValidHostNameAndUnique()
    {
        var domains = RealSchoolCatalog.Schools.Select(s => $"{s.Slug}.localhost").ToList();

        domains.Distinct().Count().ShouldBe(domains.Count);
        foreach (var domain in domains)
        {
            School.IsValidDomain(domain).ShouldBeTrue(domain);
        }
    }

    [Test]
    public void EverySlugShouldBeLowerCaseLettersDigitsAndHyphens() =>
        RealSchoolCatalog.Schools.ShouldAllBe(s => SlugPattern.IsMatch(s.Slug));

    [Test]
    public void EverySchoolShouldHaveAFittingNameAndDistinctCampuses()
    {
        foreach (var school in RealSchoolCatalog.Schools)
        {
            school.Name.Length.ShouldBeLessThanOrEqualTo(School.NameMaxLength, school.Name);
            school.Campuses.ShouldNotBeEmpty(school.Name);
            school.Campuses.Distinct().Count().ShouldBe(school.Campuses.Count, school.Name);

            foreach (var campus in school.Campuses)
            {
                campus.Trim().ShouldNotBeEmpty(school.Name);
                campus.Length.ShouldBeLessThanOrEqualTo(Branch.NameMaxLength, campus);
            }
        }
    }
}
