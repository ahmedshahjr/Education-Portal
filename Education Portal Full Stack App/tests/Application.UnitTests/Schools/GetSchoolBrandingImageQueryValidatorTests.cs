using EducationPortal.Application.Schools.Queries.GetSchoolBrandingImage;
using EducationPortal.Domain.Enums;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Application.UnitTests.Schools;

public class GetSchoolBrandingImageQueryValidatorTests
{
    private readonly GetSchoolBrandingImageQueryValidator _validator = new();

    [Test]
    public void ShouldAcceptASchoolAndAKnownKind()
    {
        _validator.Validate(new GetSchoolBrandingImageQuery(Guid.CreateVersion7(), BrandingImageKind.Favicon)).IsValid.ShouldBeTrue();
    }

    [Test]
    public void ShouldRefuseAnEmptySchoolId()
    {
        var result = _validator.Validate(new GetSchoolBrandingImageQuery(Guid.Empty, BrandingImageKind.Logo));

        result.Errors.ShouldContain(e => e.PropertyName == nameof(GetSchoolBrandingImageQuery.SchoolId));
    }

    [Test]
    public void ShouldRefuseAnUnknownKind()
    {
        _validator.Validate(new GetSchoolBrandingImageQuery(Guid.CreateVersion7(), (BrandingImageKind)99)).IsValid.ShouldBeFalse();
    }
}
