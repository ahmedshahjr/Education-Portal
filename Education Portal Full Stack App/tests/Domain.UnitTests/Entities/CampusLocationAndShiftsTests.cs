using EducationPortal.Domain.Entities;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Domain.UnitTests.Entities;

/// <summary>A campus's place on the map, its province and city, and its daily shifts.</summary>
public class CampusLocationAndShiftsTests
{
    private static readonly TimeOnly EightAm = new(8, 0);
    private static readonly TimeOnly OneThirtyPm = new(13, 30);
    private static readonly TimeOnly TwoPm = new(14, 0);
    private static readonly TimeOnly FiveThirtyPm = new(17, 30);

    private static Branch SavedCampus()
    {
        var campus = Branch.Create(3, "North", "0007");
        campus.Id = 7;
        return campus;
    }

    // ----- CampusShift -----

    [Test]
    public void ShouldCreateAShiftAndTrimItsName()
    {
        var shift = CampusShift.Create(3, 7, "  Morning  ", EightAm, OneThirtyPm);

        shift.Name.ShouldBe("Morning");
        shift.StartTime.ShouldBe(EightAm);
        shift.EndTime.ShouldBe(OneThirtyPm);
        shift.SchoolId.ShouldBe(3);
        shift.BranchId.ShouldBe(7);
    }

    [Test]
    public void ShouldRefuseAShiftThatEndsWhenItStarts()
    {
        Should.Throw<ArgumentException>(() => CampusShift.Create(3, 7, "Morning", EightAm, EightAm));
    }

    [Test]
    public void ShouldRefuseAShiftThatEndsBeforeItStarts()
    {
        Should.Throw<ArgumentException>(() => CampusShift.Create(3, 7, "Evening", TwoPm, EightAm));
    }

    [TestCase("")]
    [TestCase("   ")]
    public void ShouldRefuseAShiftWithoutAName(string name) =>
        Should.Throw<ArgumentException>(() => CampusShift.Create(3, 7, name, EightAm, OneThirtyPm));

    [Test]
    public void ShouldAcceptAShiftNameOfExactlyTheMaximumLengthAndRefuseOneOverIt()
    {
        CampusShift.Create(3, 7, new string('a', CampusShift.NameMaxLength), EightAm, OneThirtyPm).Name.Length
            .ShouldBe(CampusShift.NameMaxLength);

        Should.Throw<ArgumentOutOfRangeException>(() =>
            CampusShift.Create(3, 7, new string('a', CampusShift.NameMaxLength + 1), EightAm, OneThirtyPm));
    }

    [TestCase(0, 7)]
    [TestCase(3, 0)]
    public void ShouldRefuseAShiftWithoutASchoolOrCampus(int schoolId, int branchId) =>
        Should.Throw<ArgumentOutOfRangeException>(() => CampusShift.Create(schoolId, branchId, "Morning", EightAm, OneThirtyPm));

    // ----- Branch.SetLocation -----

    [Test]
    public void ShouldSetEveryPartOfTheLocation()
    {
        var campus = SavedCampus();

        campus.SetLocation(
            stateId: 4,
            cityId: 9,
            addressLine: "  Plot 12, Main Boulevard  ",
            latitude: 31.520370m,
            longitude: 74.358749m,
            googlePlaceId: "ChIJ-example",
            googleMapsUrl: "https://maps.google.com/?q=31.520370,74.358749");

        campus.StateId.ShouldBe(4);
        campus.CityId.ShouldBe(9);
        campus.AddressLine.ShouldBe("Plot 12, Main Boulevard");
        campus.Latitude.ShouldBe(31.520370m);
        campus.Longitude.ShouldBe(74.358749m);
        campus.GooglePlaceId.ShouldBe("ChIJ-example");
        campus.GoogleMapsUrl.ShouldBe("https://maps.google.com/?q=31.520370,74.358749");
    }

    [Test]
    public void ShouldStoreEmptyTextAsNull()
    {
        var campus = SavedCampus();

        campus.SetLocation(null, null, "   ", null, null, "", "  ");

        campus.AddressLine.ShouldBeNull();
        campus.GooglePlaceId.ShouldBeNull();
        campus.GoogleMapsUrl.ShouldBeNull();
    }

    [Test]
    public void ShouldRefuseACityWithoutItsProvince() =>
        Should.Throw<ArgumentException>(() => SavedCampus().SetLocation(null, 9, null, null, null, null, null));

    [Test]
    public void ShouldRefuseALatitudeWithoutALongitude() =>
        Should.Throw<ArgumentException>(() => SavedCampus().SetLocation(null, null, null, 31.5m, null, null, null));

    [Test]
    public void ShouldRefuseALongitudeWithoutALatitude() =>
        Should.Throw<ArgumentException>(() => SavedCampus().SetLocation(null, null, null, null, 74.3m, null, null));

    [TestCase(-90.0, 90.0)]
    [TestCase(90.0, -180.0)]
    [TestCase(0.0, 180.0)]
    [TestCase(-90.0, -180.0)]
    public void ShouldAcceptCoordinatesOnTheEdgesOfTheRange(double latitude, double longitude)
    {
        var campus = SavedCampus();

        campus.SetLocation(null, null, null, (decimal)latitude, (decimal)longitude, null, null);

        campus.Latitude.ShouldBe((decimal)latitude);
        campus.Longitude.ShouldBe((decimal)longitude);
    }

    [TestCase(90.000001, 0.0)]
    [TestCase(-90.000001, 0.0)]
    [TestCase(0.0, 180.000001)]
    [TestCase(0.0, -180.000001)]
    public void ShouldRefuseCoordinatesOutsideTheRange(double latitude, double longitude) =>
        Should.Throw<ArgumentOutOfRangeException>(() =>
            SavedCampus().SetLocation(null, null, null, (decimal)latitude, (decimal)longitude, null, null));

    [TestCase("http://maps.google.com/?q=1,2")]
    [TestCase("maps.google.com/?q=1,2")]
    [TestCase("ftp://maps.google.com/")]
    public void ShouldRefuseAGoogleMapsLinkThatIsNotHttps(string url) =>
        Should.Throw<ArgumentException>(() => SavedCampus().SetLocation(null, null, null, null, null, null, url));

    [Test]
    public void ShouldAcceptAnAddressOfExactlyTheMaximumLengthAndRefuseOneOverIt()
    {
        var campus = SavedCampus();

        campus.SetLocation(null, null, new string('a', Branch.AddressMaxLength), null, null, null, null);
        campus.AddressLine!.Length.ShouldBe(Branch.AddressMaxLength);

        Should.Throw<ArgumentOutOfRangeException>(() =>
            campus.SetLocation(null, null, new string('a', Branch.AddressMaxLength + 1), null, null, null, null));
    }

    // ----- Branch.ReplaceShifts -----

    [Test]
    public void ShouldReplaceTheShiftsOfACampus()
    {
        var campus = SavedCampus();
        var at = DateTimeOffset.UtcNow;

        campus.ReplaceShifts([("Morning", EightAm, OneThirtyPm), ("Evening", TwoPm, FiveThirtyPm)], at, "admin-1");
        campus.ReplaceShifts([("Morning", new TimeOnly(7, 30), new TimeOnly(12, 30))], at, "admin-1");

        var active = campus.Shifts.Where(s => !s.IsDeleted).ToList();
        active.ShouldHaveSingleItem().Name.ShouldBe("Morning");
        active[0].StartTime.ShouldBe(new TimeOnly(7, 30));

        // The earlier shifts are kept, deleted, for audit.
        campus.Shifts.Count(s => s.IsDeleted).ShouldBe(2);
    }

    [Test]
    public void ShouldAcceptAnEmptyListToRemoveAllShifts()
    {
        var campus = SavedCampus();
        campus.ReplaceShifts([("Morning", EightAm, OneThirtyPm)], DateTimeOffset.UtcNow, null);

        campus.ReplaceShifts([], DateTimeOffset.UtcNow, null);

        campus.Shifts.Where(s => !s.IsDeleted).ShouldBeEmpty();
    }

    [Test]
    public void ShouldAcceptFourShiftsAndRefuseFive()
    {
        var four = new[]
        {
            ("Shift 1", new TimeOnly(6, 0), new TimeOnly(7, 0)),
            ("Shift 2", new TimeOnly(8, 0), new TimeOnly(9, 0)),
            ("Shift 3", new TimeOnly(10, 0), new TimeOnly(11, 0)),
            ("Shift 4", new TimeOnly(12, 0), new TimeOnly(13, 0)),
        };
        var campus = SavedCampus();

        campus.ReplaceShifts(four, DateTimeOffset.UtcNow, null);
        campus.Shifts.Count(s => !s.IsDeleted).ShouldBe(Branch.MaxShiftsPerCampus);

        var five = four.Append(("Shift 5", new TimeOnly(14, 0), new TimeOnly(15, 0))).ToList();
        Should.Throw<ArgumentException>(() => campus.ReplaceShifts(five, DateTimeOffset.UtcNow, null));
    }

    [TestCase("Morning", "morning")]
    [TestCase("Morning", " Morning ")]
    public void ShouldRefuseTwoShiftsWithTheSameNameIgnoringCaseAndSpaces(string first, string second) =>
        Should.Throw<ArgumentException>(() => SavedCampus().ReplaceShifts(
            [(first, EightAm, OneThirtyPm), (second, TwoPm, FiveThirtyPm)],
            DateTimeOffset.UtcNow,
            null));

    [Test]
    public void ShouldRefuseShiftsOnACampusThatIsNotSavedYet() =>
        Should.Throw<InvalidOperationException>(() =>
            Branch.Create(3, "North", "0007").ReplaceShifts([("Morning", EightAm, OneThirtyPm)], DateTimeOffset.UtcNow, null));

    [Test]
    public void ShouldLeaveTheShiftsUnchangedWhenOneNewShiftIsInvalid()
    {
        var campus = SavedCampus();
        campus.ReplaceShifts([("Morning", EightAm, OneThirtyPm)], DateTimeOffset.UtcNow, null);

        Should.Throw<ArgumentException>(() =>
            campus.ReplaceShifts([("Evening", TwoPm, FiveThirtyPm), ("Broken", TwoPm, TwoPm)], DateTimeOffset.UtcNow, null));

        campus.Shifts.Where(s => !s.IsDeleted).Select(s => s.Name).ShouldBe(["Morning"]);
    }
}
