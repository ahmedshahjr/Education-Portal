using EducationPortal.Application.SchoolPublicPage.Commands.UploadSchoolPhoto;
using EducationPortal.Application.SchoolPublicPage.Common;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Application.UnitTests.SchoolPublicPage;

public class SchoolPhotoValidatorTests
{
    // The PNG signature followed by padding: enough for the type check, which reads only the first bytes.
    private static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    private static byte[] Png(int size)
    {
        var bytes = new byte[size];
        PngSignature.CopyTo(bytes, 0);
        return bytes;
    }

    [Test]
    public void ShouldAcceptAPngOfExactlyTheLimit() => SchoolPhotoRules.IsAcceptable(Png(SchoolPhoto.MaxBytes)).ShouldBeTrue();

    [Test]
    public void ShouldRefuseAPngOneByteOverTheLimit() => SchoolPhotoRules.IsAcceptable(Png(SchoolPhoto.MaxBytes + 1)).ShouldBeFalse();

    [Test]
    public void ShouldRefuseAnEmptyFile() => SchoolPhotoRules.IsAcceptable([]).ShouldBeFalse();

    [Test]
    public void ShouldRefuseNoFile() => SchoolPhotoRules.IsAcceptable(null).ShouldBeFalse();

    [Test]
    public void ShouldRefuseAFileThatOnlyLooksLikeAnImage()
    {
        var svg = "<svg xmlns=\"http://www.w3.org/2000/svg\"><script>alert(1)</script></svg>"u8.ToArray();

        SchoolPhotoRules.IsAcceptable(svg).ShouldBeFalse();
    }

    [Test]
    public void ShouldRefuseACaptionOneOverTheLimit()
    {
        var validator = new UploadSchoolPhotoCommandValidator();
        var command = new UploadSchoolPhotoCommand { Kind = PhotoKind.Campus, Content = Png(64), Caption = new string('c', SchoolPhoto.CaptionMaxLength) };

        validator.Validate(command).IsValid.ShouldBeTrue();
        validator.Validate(command with { Caption = new string('c', SchoolPhoto.CaptionMaxLength + 1) }).IsValid.ShouldBeFalse();
    }

    [Test]
    public void ShouldRefuseANegativeSortOrderAndAnEmptyCampusId()
    {
        var validator = new UploadSchoolPhotoCommandValidator();
        var command = new UploadSchoolPhotoCommand { Kind = PhotoKind.Event, Content = Png(64) };

        validator.Validate(command with { SortOrder = -1 }).IsValid.ShouldBeFalse();
        validator.Validate(command with { CampusId = Guid.Empty }).IsValid.ShouldBeFalse();
        validator.Validate(command with { CampusId = Guid.NewGuid() }).IsValid.ShouldBeTrue();
    }

    [Test]
    public void ShouldRefuseAnImageOverTheLimitInTheUploadValidator() =>
        new UploadSchoolPhotoCommandValidator()
            .Validate(new UploadSchoolPhotoCommand { Kind = PhotoKind.Campus, Content = Png(SchoolPhoto.MaxBytes + 1) })
            .IsValid.ShouldBeFalse();
}
