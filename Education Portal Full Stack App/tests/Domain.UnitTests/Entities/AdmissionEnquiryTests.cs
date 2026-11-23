using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Domain.UnitTests.Entities;

public class AdmissionEnquiryTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 15, 9, 0, 0, TimeSpan.Zero);

    private static AdmissionEnquiry Online(string phone = "+92 300 1234567", string? email = "parent@example.com", DateTimeOffset? consent = null) =>
        AdmissionEnquiry.Create(1, 1, ClassLevel.Class1, EnquiryChannel.Online, "Ali", "Imran Khan", phone, email, "Please call after 4pm.", consent ?? Now, Now);

    [Test]
    public void ShouldCreateANewOnlineEnquiryThatRecordsConsent()
    {
        var enquiry = Online();

        enquiry.Status.ShouldBe(EnquiryStatus.New);
        enquiry.ConsentGivenAt.ShouldBe(Now);
        enquiry.SubmittedAt.ShouldBe(Now);
    }

    [Test]
    public void ShouldRequireConsentForAnOnlineEnquiryButNotForAPhoneOrWalkIn()
    {
        Should.Throw<ArgumentException>(() => AdmissionEnquiry.Create(1, 1, null, EnquiryChannel.Online, "Ali", "Imran", "+92 300 1234567", null, null, null, Now));
        Should.NotThrow(() => AdmissionEnquiry.Create(1, 1, null, EnquiryChannel.Phone, "Ali", "Imran", "+92 300 1234567", null, null, null, Now));
        Should.NotThrow(() => AdmissionEnquiry.Create(1, 1, null, EnquiryChannel.InPerson, "Ali", "Imran", "+92 300 1234567", null, null, null, Now));
    }

    [TestCase("call me")]
    [TestCase("123")]
    [TestCase("")]
    public void ShouldRefuseAPhoneThatIsNotAPhone(string phone) => Should.Throw<ArgumentException>(() => Online(phone: phone));

    [TestCase("parent@example.com", true)]
    [TestCase("not-an-email", false)]
    [TestCase("Name <a@b.com>", false)]
    public void ShouldCheckTheEmail(string email, bool valid)
    {
        var act = () => Online(email: email);

        if (valid) { Should.NotThrow(act); } else { Should.Throw<ArgumentException>(act); }
    }

    [Test]
    public void ShouldStoreABlankEmailAsNullAndRefuseBlankNames()
    {
        Online(email: "  ").Email.ShouldBeNull();
        Should.Throw<ArgumentException>(() => AdmissionEnquiry.Create(1, 1, null, EnquiryChannel.Phone, " ", "Imran", "+92 300 1234567", null, null, null, Now));
        Should.Throw<ArgumentException>(() => AdmissionEnquiry.Create(1, 1, null, EnquiryChannel.Phone, "Ali", " ", "+92 300 1234567", null, null, null, Now));
    }

    [TestCase(AdmissionEnquiry.MessageMaxLength, true)]
    [TestCase(AdmissionEnquiry.MessageMaxLength + 1, false)]
    public void ShouldLimitTheMessage(int length, bool valid)
    {
        var act = () => AdmissionEnquiry.Create(1, 1, null, EnquiryChannel.Phone, "Ali", "Imran", "+92 300 1234567", null, new string('a', length), null, Now);

        if (valid) { Should.NotThrow(act); } else { Should.Throw<ArgumentOutOfRangeException>(act); }
    }

    [Test]
    public void ShouldMoveForwardAndRecordWhenTheSchoolFirstGotInTouch()
    {
        var enquiry = Online();

        enquiry.ChangeStatus(EnquiryStatus.Contacted, Now.AddHours(1));
        enquiry.ChangeStatus(EnquiryStatus.VisitScheduled, Now.AddHours(2));

        enquiry.Status.ShouldBe(EnquiryStatus.VisitScheduled);
        enquiry.ContactedAt.ShouldBe(Now.AddHours(1));
        enquiry.ClosedAt.ShouldBeNull();
    }

    [Test]
    public void ShouldAllowSkippingAStepButNotGoingBackOrRepeating()
    {
        var enquiry = Online();
        enquiry.ChangeStatus(EnquiryStatus.Applied, Now.AddHours(1));

        enquiry.ContactedAt.ShouldBe(Now.AddHours(1));
        Should.Throw<InvalidOperationException>(() => enquiry.ChangeStatus(EnquiryStatus.Contacted, Now.AddHours(2)));
        Should.Throw<InvalidOperationException>(() => enquiry.ChangeStatus(EnquiryStatus.Applied, Now.AddHours(2)));
    }

    [Test]
    public void ShouldCloseFromAnyOpenStepAndThenRefuseEveryChange()
    {
        var enquiry = Online();

        enquiry.ChangeStatus(EnquiryStatus.Closed, Now.AddDays(1));

        enquiry.ClosedAt.ShouldBe(Now.AddDays(1));
        enquiry.ContactedAt.ShouldBeNull();
        Should.Throw<InvalidOperationException>(() => enquiry.ChangeStatus(EnquiryStatus.Contacted, Now.AddDays(2)));
        Should.Throw<InvalidOperationException>(() => enquiry.AssignTo(5));
    }

    [Test]
    public void ShouldEndWhenEnrolled()
    {
        var enquiry = Online();
        enquiry.ChangeStatus(EnquiryStatus.Enrolled, Now.AddDays(3));

        enquiry.ClosedAt.ShouldBe(Now.AddDays(3));
        Should.Throw<InvalidOperationException>(() => enquiry.ChangeStatus(EnquiryStatus.Closed, Now.AddDays(4)));
    }

    [Test]
    public void ShouldAssignToAStaffMemberAndRefuseABadId()
    {
        var enquiry = Online();
        enquiry.AssignTo(7);

        enquiry.AssignedToUserDetailsId.ShouldBe(7);
        Should.Throw<ArgumentOutOfRangeException>(() => enquiry.AssignTo(0));
    }
}

public class EnquiryFollowUpAndVisitTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 15, 9, 0, 0, TimeSpan.Zero);

    private static AdmissionEnquiry Enquiry() =>
        AdmissionEnquiry.Create(3, 1, null, EnquiryChannel.Phone, "Ali", "Imran", "+92 300 1234567", null, null, null, Now);

    [Test]
    public void ShouldRecordAFollowUpForTheSameSchoolAsTheEnquiry()
    {
        var followUp = EnquiryFollowUp.Create(Enquiry(), FollowUpKind.Call, " Spoke to the father. ", Now);

        followUp.SchoolId.ShouldBe(3);
        followUp.Note.ShouldBe("Spoke to the father.");
    }

    [Test]
    public void ShouldRefuseABlankOrTooLongNoteAndAnUnknownKind()
    {
        Should.Throw<ArgumentException>(() => EnquiryFollowUp.Create(Enquiry(), FollowUpKind.Note, " ", Now));
        Should.Throw<ArgumentOutOfRangeException>(() => EnquiryFollowUp.Create(Enquiry(), FollowUpKind.Note, new string('a', EnquiryFollowUp.NoteMaxLength + 1), Now));
        Should.Throw<ArgumentOutOfRangeException>(() => EnquiryFollowUp.Create(Enquiry(), (FollowUpKind)99, "x", Now));
    }

    [Test]
    public void ShouldBookAVisitInTheFutureOnly()
    {
        var visit = CampusVisit.Request(Enquiry(), 4, Now.AddDays(2), Now);

        visit.Status.ShouldBe(VisitStatus.Requested);
        visit.SchoolId.ShouldBe(3);
        Should.Throw<ArgumentOutOfRangeException>(() => CampusVisit.Request(Enquiry(), 4, Now, Now));
        Should.Throw<ArgumentOutOfRangeException>(() => CampusVisit.Request(Enquiry(), 0, Now.AddDays(1), Now));
    }

    [Test]
    public void ShouldConfirmThenComplete()
    {
        var visit = CampusVisit.Request(Enquiry(), 4, Now.AddDays(2), Now);

        visit.Confirm();
        visit.Complete(" Liked the labs. ");

        visit.Status.ShouldBe(VisitStatus.Completed);
        visit.Notes.ShouldBe("Liked the labs.");
        Should.Throw<InvalidOperationException>(() => visit.Cancel(null));
    }

    [Test]
    public void ShouldNotCompleteAVisitThatWasNeverConfirmed()
    {
        var visit = CampusVisit.Request(Enquiry(), 4, Now.AddDays(2), Now);

        Should.Throw<InvalidOperationException>(() => visit.Complete(null));
    }

    [Test]
    public void ShouldCancelARequestedOrConfirmedVisit()
    {
        var requested = CampusVisit.Request(Enquiry(), 4, Now.AddDays(2), Now);
        requested.Cancel("Parent rang.");
        requested.Status.ShouldBe(VisitStatus.Cancelled);

        var confirmed = CampusVisit.Request(Enquiry(), 4, Now.AddDays(2), Now);
        confirmed.Confirm();
        Should.NotThrow(() => confirmed.Cancel(null));
    }

    [Test]
    public void ShouldMarkANoShowOnlyAfterTheVisitTime()
    {
        var visit = CampusVisit.Request(Enquiry(), 4, Now.AddDays(2), Now);
        visit.Confirm();

        Should.Throw<InvalidOperationException>(() => visit.MarkNoShow(Now.AddDays(1)));

        visit.MarkNoShow(Now.AddDays(2));
        visit.Status.ShouldBe(VisitStatus.NoShow);
    }

    [Test]
    public void ShouldLimitTheVisitNotes()
    {
        var visit = CampusVisit.Request(Enquiry(), 4, Now.AddDays(2), Now);
        visit.Confirm();

        Should.Throw<ArgumentOutOfRangeException>(() => visit.Complete(new string('a', CampusVisit.NotesMaxLength + 1)));
    }
}

public class SchoolReviewTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 15, 9, 0, 0, TimeSpan.Zero);

    private static SchoolReview Review(int rating = 4, string body = "Good teachers.") =>
        SchoolReview.Submit(1, 1, " Sana ", ReviewerRelation.Parent, rating, " Happy ", body, Now);

    [Test]
    public void ShouldStartPendingAndTrimTheText()
    {
        var review = Review();

        review.Status.ShouldBe(ReviewStatus.Pending);
        review.ReviewerName.ShouldBe("Sana");
        review.Title.ShouldBe("Happy");
    }

    [TestCase(1, true)]
    [TestCase(5, true)]
    [TestCase(0, false)]
    [TestCase(6, false)]
    public void ShouldKeepTheRatingFromOneToFive(int rating, bool valid)
    {
        var act = () => Review(rating);

        if (valid) { Should.NotThrow(act); } else { Should.Throw<ArgumentOutOfRangeException>(act); }
    }

    [TestCase(SchoolReview.BodyMaxLength, true)]
    [TestCase(SchoolReview.BodyMaxLength + 1, false)]
    public void ShouldLimitTheBody(int length, bool valid)
    {
        var act = () => Review(body: new string('a', length));

        if (valid) { Should.NotThrow(act); } else { Should.Throw<ArgumentOutOfRangeException>(act); }
    }

    [Test]
    public void ShouldRefuseABlankBodyOrNameAndAnUnknownRelation()
    {
        Should.Throw<ArgumentException>(() => Review(body: " "));
        Should.Throw<ArgumentException>(() => SchoolReview.Submit(1, 1, " ", ReviewerRelation.Parent, 3, null, "Ok", Now));
        Should.Throw<ArgumentOutOfRangeException>(() => SchoolReview.Submit(1, 1, "Sana", (ReviewerRelation)99, 3, null, "Ok", Now));
    }

    [Test]
    public void ShouldApproveOnceAndRecordWhoDecided()
    {
        var review = Review();

        review.Approve("user-1", Now.AddHours(1));

        review.Status.ShouldBe(ReviewStatus.Approved);
        review.ModeratedBy.ShouldBe("user-1");
        Should.Throw<InvalidOperationException>(() => review.Reject("user-2", Now.AddHours(2)));
    }

    [Test]
    public void ShouldAnswerOnlyAnApprovedReview()
    {
        var review = Review();
        Should.Throw<InvalidOperationException>(() => review.ReplyTo("Thank you.", Now));

        review.Approve("user-1", Now);
        review.ReplyTo(" Thank you. ", Now.AddDays(1));

        review.Reply.ShouldBe("Thank you.");
        review.RepliedAt.ShouldBe(Now.AddDays(1));
        Should.Throw<ArgumentOutOfRangeException>(() => review.ReplyTo(new string('a', SchoolReview.ReplyMaxLength + 1), Now));
    }

    [Test]
    public void ShouldNotAnswerARejectedReview()
    {
        var review = Review();
        review.Reject("user-1", Now);

        Should.Throw<InvalidOperationException>(() => review.ReplyTo("Thanks", Now));
    }
}
