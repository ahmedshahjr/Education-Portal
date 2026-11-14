using EducationPortal.Application.Branches.Commands.UpdateCampusLocation;
using EducationPortal.Application.Branches.Commands.UpdateCampusShifts;
using EducationPortal.Domain.Entities;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Application.UnitTests.Branches;

/// <summary>The input rules of a campus's location and shifts that need no database (SEC-INPUT-01).</summary>
public class CampusValidatorTests
{
    private static readonly TimeOnly EightAm = new(8, 0);
    private static readonly TimeOnly OneThirtyPm = new(13, 30);
    private static readonly TimeOnly TwoPm = new(14, 0);
    private static readonly TimeOnly FiveThirtyPm = new(17, 30);

    private static bool LocationIsValid(UpdateCampusLocationCommand command) =>
        new UpdateCampusLocationCommandValidator().Validate(command).IsValid;

    private static bool ShiftsAreValid(params CampusShiftInput[] shifts) =>
        new UpdateCampusShiftsCommandValidator().Validate(new UpdateCampusShiftsCommand { BranchId = Guid.NewGuid(), Shifts = shifts }).IsValid;

    // ----- Location -----

    [Test]
    public void LocationShouldAcceptACampusWithNoLocationYet() =>
        LocationIsValid(new UpdateCampusLocationCommand { BranchId = Guid.NewGuid() }).ShouldBeTrue();

    [Test]
    public void LocationShouldNeedACampus() =>
        LocationIsValid(new UpdateCampusLocationCommand { BranchId = Guid.Empty }).ShouldBeFalse();

    [Test]
    public void LocationShouldNeedBothCoordinatesOrNeither()
    {
        LocationIsValid(new UpdateCampusLocationCommand { BranchId = Guid.NewGuid(), Latitude = 31.5m, Longitude = 74.3m }).ShouldBeTrue();
        LocationIsValid(new UpdateCampusLocationCommand { BranchId = Guid.NewGuid(), Latitude = 31.5m }).ShouldBeFalse();
        LocationIsValid(new UpdateCampusLocationCommand { BranchId = Guid.NewGuid(), Longitude = 74.3m }).ShouldBeFalse();
    }

    [TestCase(-90.0, true)]
    [TestCase(90.0, true)]
    [TestCase(-90.1, false)]
    [TestCase(90.1, false)]
    public void LocationShouldKeepLatitudeInRange(double latitude, bool valid) =>
        LocationIsValid(new UpdateCampusLocationCommand { BranchId = Guid.NewGuid(), Latitude = (decimal)latitude, Longitude = 0m }).ShouldBe(valid);

    [TestCase(-180.0, true)]
    [TestCase(180.0, true)]
    [TestCase(-180.1, false)]
    [TestCase(180.1, false)]
    public void LocationShouldKeepLongitudeInRange(double longitude, bool valid) =>
        LocationIsValid(new UpdateCampusLocationCommand { BranchId = Guid.NewGuid(), Latitude = 0m, Longitude = (decimal)longitude }).ShouldBe(valid);

    [Test]
    public void LocationShouldNeedAProvinceForACity()
    {
        LocationIsValid(new UpdateCampusLocationCommand { BranchId = Guid.NewGuid(), CityId = Guid.NewGuid() }).ShouldBeFalse();
        LocationIsValid(new UpdateCampusLocationCommand { BranchId = Guid.NewGuid(), StateId = Guid.NewGuid(), CityId = Guid.NewGuid() }).ShouldBeTrue();
    }

    [TestCase("https://maps.google.com/?q=31.5,74.3", true)]
    [TestCase("http://maps.google.com/?q=31.5,74.3", false)]
    [TestCase("maps.google.com/?q=31.5,74.3", false)]
    public void LocationShouldAcceptOnlyAnHttpsGoogleMapsLink(string url, bool valid) =>
        LocationIsValid(new UpdateCampusLocationCommand { BranchId = Guid.NewGuid(), GoogleMapsUrl = url }).ShouldBe(valid);

    [Test]
    public void LocationShouldAllowNoMapsLink() =>
        LocationIsValid(new UpdateCampusLocationCommand { BranchId = Guid.NewGuid(), GoogleMapsUrl = null }).ShouldBeTrue();

    [TestCase(300, true)]
    [TestCase(301, false)]
    public void LocationShouldLimitTheAddress(int length, bool valid) =>
        LocationIsValid(new UpdateCampusLocationCommand { BranchId = Guid.NewGuid(), AddressLine = new string('a', length) }).ShouldBe(valid);

    // ----- Shifts -----

    [Test]
    public void ShiftsShouldAcceptNoShiftsAndTheMaximumOfFour()
    {
        ShiftsAreValid().ShouldBeTrue();
        ShiftsAreValid(
            new CampusShiftInput("Shift 1", EightAm, OneThirtyPm),
            new CampusShiftInput("Shift 2", TwoPm, FiveThirtyPm),
            new CampusShiftInput("Shift 3", new TimeOnly(18, 0), new TimeOnly(19, 0)),
            new CampusShiftInput("Shift 4", new TimeOnly(20, 0), new TimeOnly(21, 0))).ShouldBeTrue();
    }

    [Test]
    public void ShiftsShouldRefuseMoreThanFour() =>
        ShiftsAreValid(
            new CampusShiftInput("Shift 1", EightAm, OneThirtyPm),
            new CampusShiftInput("Shift 2", TwoPm, FiveThirtyPm),
            new CampusShiftInput("Shift 3", new TimeOnly(18, 0), new TimeOnly(19, 0)),
            new CampusShiftInput("Shift 4", new TimeOnly(20, 0), new TimeOnly(21, 0)),
            new CampusShiftInput("Shift 5", new TimeOnly(22, 0), new TimeOnly(23, 0))).ShouldBeFalse();

    [TestCase("Morning", "morning")]
    [TestCase("Morning", " Morning ")]
    public void ShiftsShouldRefuseTwoNamesThatMatchIgnoringCaseAndSpaces(string first, string second) =>
        ShiftsAreValid(
            new CampusShiftInput(first, EightAm, OneThirtyPm),
            new CampusShiftInput(second, TwoPm, FiveThirtyPm)).ShouldBeFalse();

    [TestCase("", false)]
    [TestCase("   ", false)]
    [TestCase("Morning", true)]
    public void ShiftsShouldNeedAName(string name, bool valid) =>
        ShiftsAreValid(new CampusShiftInput(name, EightAm, OneThirtyPm)).ShouldBe(valid);

    [TestCase(50, true)]
    [TestCase(51, false)]
    public void ShiftsShouldLimitTheName(int length, bool valid) =>
        ShiftsAreValid(new CampusShiftInput(new string('a', length), EightAm, OneThirtyPm)).ShouldBe(valid);

    [Test]
    public void ShiftsShouldEndAfterTheyStart()
    {
        ShiftsAreValid(new CampusShiftInput("Morning", EightAm, OneThirtyPm)).ShouldBeTrue();
        ShiftsAreValid(new CampusShiftInput("Morning", EightAm, EightAm)).ShouldBeFalse();
        ShiftsAreValid(new CampusShiftInput("Morning", OneThirtyPm, EightAm)).ShouldBeFalse();
    }

    [Test]
    public void ShiftsShouldNeedACampus() =>
        new UpdateCampusShiftsCommandValidator().Validate(new UpdateCampusShiftsCommand { BranchId = Guid.Empty }).IsValid.ShouldBeFalse();
}
