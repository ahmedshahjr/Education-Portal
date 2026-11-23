using EducationPortal.Application.SchoolPublicPage.Commands.CreateClassOffering;
using EducationPortal.Application.SchoolPublicPage.Commands.UpdateClassOffering;
using EducationPortal.Domain.Enums;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Application.UnitTests.SchoolPublicPage;

public class ClassOfferingValidatorTests
{
    private static CreateClassOfferingCommand Valid() => new()
    {
        CampusId = Guid.NewGuid(),
        Level = ClassLevel.Nursery,
        AcademicYear = "2026-27",
        MonthlyFee = 5000m,
        AdmissionFee = 1000m,
        CurrencyCode = "PKR",
        SeatsTotal = 40,
        SeatsAvailable = 40,
        Status = AdmissionStatus.Open,
        ShowFee = true,
    };

    [TestCase("2026", true)]
    [TestCase("2026-27", true)]
    [TestCase("26-27", false)]
    [TestCase("2026/27", false)]
    [TestCase("", false)]
    public void ShouldCheckTheAcademicYearFormat(string year, bool valid) =>
        new CreateClassOfferingCommandValidator().Validate(Valid() with { AcademicYear = year }).IsValid.ShouldBe(valid);

    [TestCase("PKR", true)]
    [TestCase("pkr", true)]
    [TestCase("PK", false)]
    [TestCase("PKRS", false)]
    public void ShouldCheckTheCurrencyFormat(string currency, bool valid) =>
        new CreateClassOfferingCommandValidator().Validate(Valid() with { CurrencyCode = currency }).IsValid.ShouldBe(valid);

    [TestCase(0, true)]
    [TestCase(-1, false)]
    public void ShouldRefuseANegativeMonthlyFee(decimal fee, bool valid) =>
        new CreateClassOfferingCommandValidator().Validate(Valid() with { MonthlyFee = fee }).IsValid.ShouldBe(valid);

    [TestCase(null, true)]
    [TestCase(0, true)]
    [TestCase(-1, false)]
    public void ShouldRefuseANegativeAdmissionFee(decimal? fee, bool valid) =>
        new CreateClassOfferingCommandValidator().Validate(Valid() with { AdmissionFee = fee }).IsValid.ShouldBe(valid);

    [TestCase(40, true)]
    [TestCase(41, false)]
    [TestCase(0, true)]
    public void ShouldAllowNoMoreFreeSeatsThanTheTotal(int available, bool valid) =>
        new CreateClassOfferingCommandValidator().Validate(Valid() with { SeatsAvailable = available }).IsValid.ShouldBe(valid);

    [Test]
    public void ShouldRefuseNegativeSeats() =>
        new CreateClassOfferingCommandValidator().Validate(Valid() with { SeatsTotal = -1, SeatsAvailable = -1 }).IsValid.ShouldBeFalse();

    [Test]
    public void ShouldRefuseUndefinedLevelAndStatus()
    {
        var validator = new CreateClassOfferingCommandValidator();
        validator.Validate(Valid() with { Level = (ClassLevel)99 }).IsValid.ShouldBeFalse();
        validator.Validate(Valid() with { Status = (AdmissionStatus)99 }).IsValid.ShouldBeFalse();
    }

    [Test]
    public void ShouldApplyTheSameRulesOnUpdate()
    {
        var update = new UpdateClassOfferingCommandValidator();
        var command = new UpdateClassOfferingCommand
        {
            OfferingId = Guid.NewGuid(),
            AcademicYear = "2026-27",
            MonthlyFee = 5500m,
            CurrencyCode = "PKR",
            SeatsTotal = 40,
            SeatsAvailable = 35,
            Status = AdmissionStatus.WaitingList,
        };

        update.Validate(command).IsValid.ShouldBeTrue();
        update.Validate(command with { SeatsAvailable = 41 }).IsValid.ShouldBeFalse();
        update.Validate(command with { OfferingId = Guid.Empty }).IsValid.ShouldBeFalse();
    }
}
