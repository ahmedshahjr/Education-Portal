using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Domain.UnitTests.Entities;

public class SchoolRequestTests
{
    [Test]
    public void ShouldStartPendingWithTrimmedFieldsAndNormalizedDomain()
    {
        var request = SchoolRequest.Submit("  Greenfield School ", " Greenfield.EXAMPLE. ", " Ayesha Khan ", " ayesha@greenfield.example ");

        request.SchoolName.ShouldBe("Greenfield School");
        request.RequestedDomain.ShouldBe("greenfield.example");
        request.ContactName.ShouldBe("Ayesha Khan");
        request.ContactEmail.ShouldBe("ayesha@greenfield.example");
        request.Status.ShouldBe(SchoolRequestStatus.Pending);
    }

    [Test]
    public void ShouldRefuseAnInvalidDomain()
    {
        Should.Throw<ArgumentException>(() => SchoolRequest.Submit("Greenfield", "https://greenfield.example/path", "Ayesha", "ayesha@greenfield.example"));
    }

    [Test]
    public void ShouldRefuseAnEmailThatIsNotAnAddress()
    {
        Should.Throw<ArgumentException>(() => SchoolRequest.Submit("Greenfield", "greenfield.example", "Ayesha", "not-an-email"));
        Should.Throw<ArgumentException>(() => SchoolRequest.Submit("Greenfield", "greenfield.example", "Ayesha", "@greenfield.example"));
    }

    [Test]
    public void ShouldRefuseANameOverTheLengthLimit()
    {
        var longName = new string('x', SchoolRequest.SchoolNameMaxLength + 1);

        Should.Throw<ArgumentOutOfRangeException>(() => SchoolRequest.Submit(longName, "greenfield.example", "Ayesha", "ayesha@greenfield.example"));
    }

    [Test]
    public void ShouldApproveOnlyWhilePending()
    {
        var request = SchoolRequest.Submit("Greenfield", "greenfield.example", "Ayesha", "ayesha@greenfield.example");

        request.Approve();

        request.Status.ShouldBe(SchoolRequestStatus.Approved);
        Should.Throw<InvalidOperationException>(() => request.Reject());
        Should.Throw<InvalidOperationException>(() => request.Approve());
    }

    [Test]
    public void ShouldRejectOnlyWhilePending()
    {
        var request = SchoolRequest.Submit("Greenfield", "greenfield.example", "Ayesha", "ayesha@greenfield.example");

        request.Reject();

        request.Status.ShouldBe(SchoolRequestStatus.Rejected);
        Should.Throw<InvalidOperationException>(() => request.Approve());
    }
}
