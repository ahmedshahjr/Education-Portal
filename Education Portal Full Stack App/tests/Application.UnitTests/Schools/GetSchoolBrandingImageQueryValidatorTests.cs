using EducationPortal.Application.Schools.Queries.GetSchoolBrandingImage;
using EducationPortal.Domain.Enums;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Application.UnitTests.Schools;

public class GetSchoolBrandingImageQueryValidatorTests
{
    private readonly GetSchoolBrandingImageQueryValidator _validator = new();

    [Test]
    public void ShouldAcceptAPositiveSchoolAndAKnownKind()
    {
        _validator.Validate(new GetSchoolBrandingImageQuery(1, BrandingImageKind.Favicon)).IsValid.ShouldBeTrue();
    }

    [Test]
    public void ShouldRefuseASchoolIdThatIsNotPositive()
    {
        var result = _validator.Validate(new GetSchoolBrandingImageQuery(0, BrandingImageKind.Logo));

        result.Errors.ShouldContain(e => e.PropertyName == nameof(GetSchoolBrandingImageQuery.SchoolId));
    }

    [Test]
    public void ShouldRefuseAnUnknownKind()
    {
        _validator.Validate(new GetSchoolBrandingImageQuery(1, (BrandingImageKind)99)).IsValid.ShouldBeFalse();
    }
}
