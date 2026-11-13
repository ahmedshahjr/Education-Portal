using EducationPortal.Domain.Entities;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Domain.UnitTests.Entities;

public class CityTests
{
    [Test]
    public void ShouldCreateACityFromTheDataset()
    {
        var city = City.FromDataset(7, " Karachi ", 24.8608m, 67.0104m);

        city.StateId.ShouldBe(7);
        city.Name.ShouldBe("Karachi");
        city.NativeName.ShouldBeNull();
        city.Latitude.ShouldBe(24.8608m);
        city.Longitude.ShouldBe(67.0104m);
        city.IsActive.ShouldBeTrue();
        city.IsCustom.ShouldBeFalse();
    }

    [Test]
    public void ShouldAcceptMissingCoordinatesAndTheBoundaries()
    {
        City.FromDataset(1, "Nowhere", null, null).Latitude.ShouldBeNull();

        var corner = City.FromDataset(1, "Corner", -90m, 180m);
        corner.Latitude.ShouldBe(-90m);
        corner.Longitude.ShouldBe(180m);
    }

    [TestCase(90.0001, 0)]
    [TestCase(-90.0001, 0)]
    [TestCase(0, 180.0001)]
    [TestCase(0, -180.0001)]
    public void ShouldRefuseCoordinatesOutsideTheGlobe(double latitude, double longitude)
    {
        Should.Throw<ArgumentOutOfRangeException>(() => City.FromDataset(1, "Somewhere", (decimal)latitude, (decimal)longitude));
    }

    [Test]
    public void ShouldRefuseANameThatIsEmptyOrTooLong()
    {
        Should.Throw<ArgumentException>(() => City.FromDataset(1, "", null, null));
        Should.Throw<ArgumentOutOfRangeException>(() => City.FromDataset(1, new string('a', City.NameMaxLength + 1), null, null));
        City.FromDataset(1, new string('a', City.NameMaxLength), null, null).Name.Length.ShouldBe(City.NameMaxLength);
    }

    [Test]
    public void ShouldUpdateADatasetCityButNeverACustomOne()
    {
        var city = City.FromDataset(1, "Lahore", 31.5m, 74.3m);
        city.UpdateFromDataset(31.55m, 74.34m);
        city.Latitude.ShouldBe(31.55m);

        var custom = City.CreateCustom(1, "Chak 45", "چک 45", 31.1m, 73.1m);
        custom.IsCustom.ShouldBeTrue();
        custom.NativeName.ShouldBe("چک 45");
        Should.Throw<InvalidOperationException>(() => custom.UpdateFromDataset(0m, 0m));
        custom.Latitude.ShouldBe(31.1m);
    }
}
