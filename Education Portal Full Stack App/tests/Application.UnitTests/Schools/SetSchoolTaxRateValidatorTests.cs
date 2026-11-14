using EducationPortal.Application.Schools.Commands.SetSchoolTaxRate;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Application.UnitTests.Schools;

public class SetSchoolTaxRateValidatorTests
{
    private readonly SetSchoolTaxRateCommandValidator _validator = new();

    [TestCase(null, true)]
    [TestCase(0, true)]
    [TestCase(15.5, true)]
    [TestCase(100, true)]
    [TestCase(-1, false)]
    [TestCase(100.01, false)]
    [TestCase(1.234, false)]
    public void ShouldCheckTheRate(double? rate, bool valid) =>
        _validator.Validate(new SetSchoolTaxRateCommand { SchoolId = Guid.NewGuid(), TaxRate = rate is null ? null : (decimal)rate }).IsValid.ShouldBe(valid);

    [Test]
    public void ShouldNeedASchool() =>
        _validator.Validate(new SetSchoolTaxRateCommand { SchoolId = Guid.Empty, TaxRate = 5 }).IsValid.ShouldBeFalse();
}
