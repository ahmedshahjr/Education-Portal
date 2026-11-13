using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Domain.UnitTests.Entities;

public class StateTests
{
    [Test]
    public void ShouldCreateAStateFromTheDatasetWithANormalisedIsoCode()
    {
        var state = State.FromDataset(1, "pk-sd", " Sindh ", " سندھ ", StateType.Province);

        state.IsoCode.ShouldBe("PK-SD");
        state.Name.ShouldBe("Sindh");
        state.NativeName.ShouldBe("سندھ");
        state.Type.ShouldBe(StateType.Province);
        state.IsActive.ShouldBeTrue();
        state.IsCustom.ShouldBeFalse();
    }

    [TestCase("")]
    [TestCase("PKSD")]
    [TestCase("P-SD")]
    [TestCase("PK-SIND")]
    [TestCase("PK-S D")]
    public void ShouldRefuseAnInvalidIsoCode(string isoCode)
    {
        Should.Throw<ArgumentException>(() => State.FromDataset(1, isoCode, "Sindh", null, StateType.Province));
    }

    [Test]
    public void ShouldAcceptTheLongestIsoCodeAndName()
    {
        var name = new string('a', State.NameMaxLength);

        var state = State.FromDataset(1, "US-123", name, null, StateType.State);

        state.Name.ShouldBe(name);
        state.IsoCode.ShouldBe("US-123");
    }

    [Test]
    public void ShouldRefuseANameThatIsEmptyOrTooLong()
    {
        Should.Throw<ArgumentException>(() => State.FromDataset(1, "PK-SD", " ", null, StateType.Province));
        Should.Throw<ArgumentOutOfRangeException>(() => State.FromDataset(1, "PK-SD", new string('a', State.NameMaxLength + 1), null, StateType.Province));
        Should.Throw<ArgumentOutOfRangeException>(() => State.FromDataset(1, "PK-SD", "Sindh", new string('a', State.NativeNameMaxLength + 1), StateType.Province));
    }

    [Test]
    public void ShouldStoreABlankNativeNameAsNull()
    {
        State.FromDataset(1, "PK-SD", "Sindh", "  ", StateType.Province).NativeName.ShouldBeNull();
    }

    [Test]
    public void ShouldUpdateADatasetStateButNeverACustomOne()
    {
        var state = State.FromDataset(1, "PK-IS", "Islamabad", null, StateType.FederalCapitalTerritory);
        state.UpdateFromDataset("Islamabad Capital Territory", "اسلام آباد", StateType.FederalCapitalTerritory);
        state.Name.ShouldBe("Islamabad Capital Territory");
        state.IsoCode.ShouldBe("PK-IS");

        var custom = State.CreateCustom(1, "New Region", null, StateType.Region);
        custom.IsCustom.ShouldBeTrue();
        custom.IsoCode.ShouldBeNull();
        Should.Throw<InvalidOperationException>(() => custom.UpdateFromDataset("Other", null, StateType.Region));
        custom.Name.ShouldBe("New Region");
    }

    [Test]
    public void ShouldSwitchActiveOnAndOff()
    {
        var state = State.FromDataset(1, "PK-SD", "Sindh", null, StateType.Province);

        state.Deactivate();
        state.IsActive.ShouldBeFalse();
        state.Activate();
        state.IsActive.ShouldBeTrue();
    }
}
