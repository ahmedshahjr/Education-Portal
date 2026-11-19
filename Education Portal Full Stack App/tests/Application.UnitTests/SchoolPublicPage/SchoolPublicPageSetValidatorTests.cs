using EducationPortal.Application.SchoolPublicPage.Commands.SetCurricula;
using EducationPortal.Application.SchoolPublicPage.Commands.SetFacility;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Application.UnitTests.SchoolPublicPage;

public class SchoolPublicPageSetValidatorTests
{
    [Test]
    public void ShouldAcceptAnEmptyCurriculaSetWhichClearsIt() =>
        new SetCurriculaCommandValidator().Validate(new SetCurriculaCommand { Kinds = [] }).IsValid.ShouldBeTrue();

    [Test]
    public void ShouldAcceptDistinctCurricula() =>
        new SetCurriculaCommandValidator().Validate(new SetCurriculaCommand { Kinds = [CurriculumKind.Matric, CurriculumKind.Cambridge] }).IsValid.ShouldBeTrue();

    [Test]
    public void ShouldRefuseACurriculumListedTwice() =>
        new SetCurriculaCommandValidator().Validate(new SetCurriculaCommand { Kinds = [CurriculumKind.Cambridge, CurriculumKind.Cambridge] }).IsValid.ShouldBeFalse();

    [Test]
    public void ShouldRefuseAnUndefinedCurriculum() =>
        new SetCurriculaCommandValidator().Validate(new SetCurriculaCommand { Kinds = [(CurriculumKind)99] }).IsValid.ShouldBeFalse();

    [Test]
    public void ShouldAcceptAFacilityNoteAtTheLimitAndRefuseOneOverIt()
    {
        var validator = new SetFacilityCommandValidator();
        validator.Validate(new SetFacilityCommand { Kind = FacilityKind.Transport, Description = new string('d', SchoolFacility.DescriptionMaxLength) }).IsValid.ShouldBeTrue();
        validator.Validate(new SetFacilityCommand { Kind = FacilityKind.Transport, Description = new string('d', SchoolFacility.DescriptionMaxLength + 1) }).IsValid.ShouldBeFalse();
    }

    [Test]
    public void ShouldAcceptAFacilityWithoutANote() =>
        new SetFacilityCommandValidator().Validate(new SetFacilityCommand { Kind = FacilityKind.Library }).IsValid.ShouldBeTrue();

    [Test]
    public void ShouldRefuseAnUndefinedFacility() =>
        new SetFacilityCommandValidator().Validate(new SetFacilityCommand { Kind = (FacilityKind)99 }).IsValid.ShouldBeFalse();
}
