using EducationPortal.Application.Users.Commands.SetPersonCampuses;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Application.UnitTests.Users;

public class SetPersonCampusesCommandValidatorTests
{
    private readonly SetPersonCampusesCommandValidator _validator = new();

    [Test]
    public void ShouldAcceptDistinctCampusesAndNoCampusAtAll()
    {
        _validator.Validate(new SetPersonCampusesCommand { PersonId = Guid.NewGuid(), CampusIds = [Guid.NewGuid(), Guid.NewGuid()] }).IsValid.ShouldBeTrue();
        _validator.Validate(new SetPersonCampusesCommand { PersonId = Guid.NewGuid(), CampusIds = [] }).IsValid.ShouldBeTrue();
    }

    [Test]
    public void ShouldRefuseAMissingPerson() =>
        _validator.Validate(new SetPersonCampusesCommand { PersonId = Guid.Empty, CampusIds = [] }).IsValid.ShouldBeFalse();

    [Test]
    public void ShouldRefuseACampusListedTwice()
    {
        var id = Guid.NewGuid();

        _validator.Validate(new SetPersonCampusesCommand { PersonId = Guid.NewGuid(), CampusIds = [id, id] }).IsValid.ShouldBeFalse();
    }

    [TestCase(SetPersonCampusesCommandValidator.MaxCampuses, true)]
    [TestCase(SetPersonCampusesCommandValidator.MaxCampuses + 1, false)]
    public void ShouldLimitTheList(int count, bool valid)
    {
        var campuses = Enumerable.Range(0, count).Select(_ => Guid.NewGuid()).ToList();

        _validator.Validate(new SetPersonCampusesCommand { PersonId = Guid.NewGuid(), CampusIds = campuses }).IsValid.ShouldBe(valid);
    }
}
