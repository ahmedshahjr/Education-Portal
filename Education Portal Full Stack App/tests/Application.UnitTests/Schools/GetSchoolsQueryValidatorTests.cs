using EducationPortal.Application.Schools.Queries.GetSchools;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Application.UnitTests.Schools;

public class GetSchoolsQueryValidatorTests
{
    private readonly GetSchoolsQueryValidator _validator = new();

    [Test]
    public void ShouldAcceptAQueryWithoutFilters()
    {
        _validator.Validate(new GetSchoolsQuery()).IsValid.ShouldBeTrue();
    }

    [Test]
    public void ShouldAcceptASearchAndACountry()
    {
        _validator.Validate(new GetSchoolsQuery { Search = "academy", CountryId = Guid.CreateVersion7() }).IsValid.ShouldBeTrue();
    }

    [Test]
    public void ShouldAcceptTheLongestSearch()
    {
        var query = new GetSchoolsQuery { Search = new string('a', GetSchoolsQuery.MaxSearchLength) };

        _validator.Validate(query).IsValid.ShouldBeTrue();
    }

    [Test]
    public void ShouldRefuseASearchThatIsTooLong()
    {
        var query = new GetSchoolsQuery { Search = new string('a', GetSchoolsQuery.MaxSearchLength + 1) };

        _validator.Validate(query).Errors.ShouldContain(e => e.PropertyName == nameof(GetSchoolsQuery.Search));
    }

    [Test]
    public void ShouldRefuseAnEmptyCountryId()
    {
        var query = new GetSchoolsQuery { CountryId = Guid.Empty };

        _validator.Validate(query).Errors.ShouldContain(e => e.PropertyName == nameof(GetSchoolsQuery.CountryId));
    }

    [TestCase(0, 25)]
    [TestCase(1, 0)]
    public void ShouldRefuseAnInvalidPage(int pageNumber, int pageSize)
    {
        _validator.Validate(new GetSchoolsQuery { PageNumber = pageNumber, PageSize = pageSize }).IsValid.ShouldBeFalse();
    }
}
