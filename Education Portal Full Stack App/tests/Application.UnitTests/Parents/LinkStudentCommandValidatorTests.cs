using EducationPortal.Application.Parents.Commands.LinkStudent;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Application.UnitTests.Parents;

public class LinkStudentCommandValidatorTests
{
    private readonly LinkStudentCommandValidator _validator = new();

    [Test]
    public void ShouldAcceptAParentAndADifferentStudent()
    {
        var result = _validator.Validate(new LinkStudentCommand { ParentId = Guid.NewGuid(), StudentId = Guid.NewGuid() });

        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public void ShouldRefuseAPersonAsTheirOwnChild()
    {
        var id = Guid.NewGuid();

        var result = _validator.Validate(new LinkStudentCommand { ParentId = id, StudentId = id });

        result.Errors.ShouldContain(e => e.PropertyName == nameof(LinkStudentCommand.StudentId));
    }

    [Test]
    public void ShouldRefuseAnEmptyParent()
    {
        var result = _validator.Validate(new LinkStudentCommand { ParentId = Guid.Empty, StudentId = Guid.NewGuid() });

        result.Errors.ShouldContain(e => e.PropertyName == nameof(LinkStudentCommand.ParentId));
    }
}
