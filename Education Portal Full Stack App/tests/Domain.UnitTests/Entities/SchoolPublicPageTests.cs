using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Domain.UnitTests.Entities;

public class SchoolPublicProfileTests
{
    private static readonly DateOnly Today = new(2026, 11, 15);
    private static readonly DateTimeOffset Now = new(2026, 11, 15, 9, 0, 0, TimeSpan.Zero);

    private static SchoolProfile Filled()
    {
        var profile = SchoolProfile.Create(1);
        profile.Update("A school for every child", "We teach from play group to matric.", 1998, SchoolGenderPolicy.CoEducation, " Mon to Fri, 8 to 2 ", "https://tour.example.com", Today);
        return profile;
    }

    [Test]
    public void ShouldStartAsAnUnpublishedDraft()
    {
        var profile = SchoolProfile.Create(1);

        profile.IsPublished.ShouldBeFalse();
        profile.SchoolId.ShouldBe(1);
        profile.GenderPolicy.ShouldBe(SchoolGenderPolicy.CoEducation);
    }

    [Test]
    public void ShouldRefuseASchoolIdThatIsNotPositive() => Should.Throw<ArgumentOutOfRangeException>(() => SchoolProfile.Create(0));

    [Test]
    public void ShouldTrimTheTextAndStoreBlankOptionalsAsNull()
    {
        var profile = SchoolProfile.Create(1);

        profile.Update("  Tag ", " About ", null, SchoolGenderPolicy.GirlsOnly, "  ", "   ", Today);

        profile.Tagline.ShouldBe("Tag");
        profile.About.ShouldBe("About");
        profile.VisitingHours.ShouldBeNull();
        profile.VirtualTourUrl.ShouldBeNull();
    }

    [TestCase(SchoolProfile.TaglineMaxLength, true)]
    [TestCase(SchoolProfile.TaglineMaxLength + 1, false)]
    public void ShouldLimitTheTagline(int length, bool valid)
    {
        var act = () => SchoolProfile.Create(1).Update(new string('a', length), "About", null, SchoolGenderPolicy.CoEducation, null, null, Today);

        if (valid) { Should.NotThrow(act); } else { Should.Throw<ArgumentOutOfRangeException>(act); }
    }

    [TestCase(SchoolProfile.MinimumEstablishedYear, true)]
    [TestCase(SchoolProfile.MinimumEstablishedYear - 1, false)]
    [TestCase(2026, true)]
    [TestCase(2027, false)]
    public void ShouldLimitTheEstablishedYear(int year, bool valid)
    {
        var act = () => SchoolProfile.Create(1).Update("Tag", "About", year, SchoolGenderPolicy.CoEducation, null, null, Today);

        if (valid) { Should.NotThrow(act); } else { Should.Throw<ArgumentOutOfRangeException>(act); }
    }

    [TestCase("ftp://tour.example.com")]
    [TestCase("not a url")]
    public void ShouldRefuseAVirtualTourThatIsNotAWebAddress(string url) =>
        Should.Throw<ArgumentException>(() => SchoolProfile.Create(1).Update("Tag", "About", null, SchoolGenderPolicy.CoEducation, null, url, Today));

    [Test]
    public void ShouldPublishOnlyWhenItHasATaglineAndAnAboutText()
    {
        Should.Throw<InvalidOperationException>(() => SchoolProfile.Create(1).Publish(Now));

        var profile = Filled();
        profile.Publish(Now);

        profile.IsPublished.ShouldBeTrue();
        profile.PublishedAt.ShouldBe(Now);
    }

    [Test]
    public void ShouldKeepTheFirstPublishTimeWhenPublishedAgainAndClearItWhenUnpublished()
    {
        var profile = Filled();
        profile.Publish(Now);
        profile.Publish(Now.AddDays(1));

        profile.PublishedAt.ShouldBe(Now);

        profile.Unpublish();
        profile.IsPublished.ShouldBeFalse();
        profile.PublishedAt.ShouldBeNull();
    }
}

public class SchoolCurriculumAndFacilityTests
{
    [Test]
    public void ShouldCreateACurriculum() => SchoolCurriculum.Create(1, CurriculumKind.Cambridge).Kind.ShouldBe(CurriculumKind.Cambridge);

    [Test]
    public void ShouldRefuseAnUnknownCurriculumOrSchool()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => SchoolCurriculum.Create(1, (CurriculumKind)99));
        Should.Throw<ArgumentOutOfRangeException>(() => SchoolCurriculum.Create(0, CurriculumKind.Matric));
    }

    [TestCase(SchoolFacility.DescriptionMaxLength, true)]
    [TestCase(SchoolFacility.DescriptionMaxLength + 1, false)]
    public void ShouldLimitTheFacilityNote(int length, bool valid)
    {
        var act = () => SchoolFacility.Create(1, FacilityKind.Transport, new string('a', length));

        if (valid) { Should.NotThrow(act); } else { Should.Throw<ArgumentOutOfRangeException>(act); }
    }

    [Test]
    public void ShouldStoreABlankFacilityNoteAsNullAndRefuseAnUnknownKind()
    {
        SchoolFacility.Create(1, FacilityKind.Library, "  ").Description.ShouldBeNull();
        Should.Throw<ArgumentOutOfRangeException>(() => SchoolFacility.Create(1, (FacilityKind)99, null));
    }
}

public class ClassOfferingTests
{
    private static ClassOffering Offering(int total = 30, int available = 10, decimal fee = 5000m, string year = "2026-27", string currency = "pkr") =>
        ClassOffering.Create(1, null, ClassLevel.Class5, year, fee, 10000m, currency, total, available, AdmissionStatus.Open, showFee: true);

    [Test]
    public void ShouldCreateAnOfferingAndUpperCaseTheCurrency()
    {
        var offering = Offering();

        offering.CurrencyCode.ShouldBe("PKR");
        offering.CampusId.ShouldBeNull();
        offering.SeatsAvailable.ShouldBe(10);
    }

    [TestCase("2026", true)]
    [TestCase("2026-27", true)]
    [TestCase("26-27", false)]
    [TestCase("2026-2027", false)]
    [TestCase("", false)]
    public void ShouldCheckTheAcademicYear(string year, bool valid)
    {
        var act = () => Offering(year: year);

        if (valid) { Should.NotThrow(act); } else { Should.Throw<ArgumentException>(act); }
    }

    [TestCase(30, 30, true)]
    [TestCase(30, 31, false)]
    [TestCase(0, 0, true)]
    [TestCase(-1, 0, false)]
    [TestCase(10, -1, false)]
    public void ShouldKeepTheFreeSeatsWithinTheTotal(int total, int available, bool valid)
    {
        var act = () => Offering(total, available);

        if (valid) { Should.NotThrow(act); } else { Should.Throw<ArgumentOutOfRangeException>(act); }
    }

    [Test]
    public void ShouldAcceptAFeeOfZeroAndRefuseANegativeFee()
    {
        Should.NotThrow(() => Offering(fee: 0m));
        Should.Throw<ArgumentOutOfRangeException>(() => Offering(fee: -1m));
    }

    [TestCase("PK")]
    [TestCase("PKRS")]
    [TestCase("12A")]
    public void ShouldRefuseACurrencyThatIsNotThreeLetters(string currency) => Should.Throw<ArgumentException>(() => Offering(currency: currency));

    [Test]
    public void ShouldRefuseAnUnknownClassAndANonPositiveCampus()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => ClassOffering.Create(1, null, (ClassLevel)99, "2026", 1m, null, "PKR", 1, 1, AdmissionStatus.Open, true));
        Should.Throw<ArgumentOutOfRangeException>(() => ClassOffering.Create(1, 0, ClassLevel.Nursery, "2026", 1m, null, "PKR", 1, 1, AdmissionStatus.Open, true));
    }

    [Test]
    public void ShouldChangeAnOffering()
    {
        var offering = Offering();
        offering.Change("2027-28", 6000m, null, "USD", 40, 40, AdmissionStatus.WaitingList, showFee: false);

        offering.AcademicYear.ShouldBe("2027-28");
        offering.Status.ShouldBe(AdmissionStatus.WaitingList);
        offering.ShowFee.ShouldBeFalse();
    }
}

public class SchoolPhotoTests
{
    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0, 0, 0, 0];

    [Test]
    public void ShouldKeepAPngAndDetectItsTypeFromTheBytes()
    {
        var photo = SchoolPhoto.Create(1, null, PhotoKind.Campus, " Front gate ", 2, Png);

        photo.ContentType.ShouldBe("image/png");
        photo.Caption.ShouldBe("Front gate");
        photo.Version.Length.ShouldBe(SchoolPhoto.VersionLength);
    }

    [Test]
    public void ShouldRefuseEmptyTooLargeAndNonImageFiles()
    {
        Should.Throw<ArgumentException>(() => SchoolPhoto.Create(1, null, PhotoKind.Event, null, 0, []));
        Should.Throw<ArgumentException>(() => SchoolPhoto.Create(1, null, PhotoKind.Event, null, 0, "<svg onload=alert(1)/>"u8.ToArray()));

        var tooLarge = new byte[SchoolPhoto.MaxBytes + 1];
        Png.CopyTo(tooLarge, 0);
        Should.Throw<ArgumentException>(() => SchoolPhoto.Create(1, null, PhotoKind.Event, null, 0, tooLarge));
    }

    [Test]
    public void ShouldAcceptAFileOfExactlyTheMaximumSize()
    {
        var content = new byte[SchoolPhoto.MaxBytes];
        Png.CopyTo(content, 0);

        Should.NotThrow(() => SchoolPhoto.Create(1, null, PhotoKind.Event, null, 0, content));
    }

    [Test]
    public void ShouldRefuseANegativeOrderAndALongCaption()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => SchoolPhoto.Create(1, null, PhotoKind.Event, null, -1, Png));
        Should.Throw<ArgumentOutOfRangeException>(() => SchoolPhoto.Create(1, null, PhotoKind.Event, new string('a', SchoolPhoto.CaptionMaxLength + 1), 0, Png));
    }

    [Test]
    public void ShouldChangeTheVersionWhenTheContentChanges()
    {
        var photo = SchoolPhoto.Create(1, null, PhotoKind.Campus, null, 0, Png);
        var before = photo.Version;

        photo.Replace([0xFF, 0xD8, 0xFF, 0xE0, 1, 2, 3]);

        photo.ContentType.ShouldBe("image/jpeg");
        photo.Version.ShouldNotBe(before);
    }
}
