using EducationPortal.Domain.Communication;
using EducationPortal.Domain.Enums;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Domain.UnitTests.Communication;

/// <summary>Who an event reaches: staff are everyone who is not a student or a parent; an account with no person reaches only "All".</summary>
public class AudienceRulesTests
{
    [TestCase(UserType.Principal, true, false, false)]
    [TestCase(UserType.Teacher, true, false, false)]
    [TestCase(UserType.Staff, true, false, false)]
    [TestCase(UserType.Parent, false, true, false)]
    [TestCase(UserType.Student, false, false, true)]
    public void ShouldReachStaffParentsAndStudentsByType(UserType type, bool staff, bool parents, bool students)
    {
        AudienceRules.Reaches(EventAudience.Staff, type).ShouldBe(staff);
        AudienceRules.Reaches(EventAudience.Parents, type).ShouldBe(parents);
        AudienceRules.Reaches(EventAudience.Students, type).ShouldBe(students);
        AudienceRules.Reaches(EventAudience.All, type).ShouldBeTrue();
    }

    [Test]
    public void ShouldShowAnAccountWithNoPersonOnlyEventsForEveryone()
    {
        AudienceRules.Visible(EventAudience.All, null).ShouldBeTrue();
        AudienceRules.Visible(EventAudience.Staff, null).ShouldBeFalse();
        AudienceRules.Visible(EventAudience.Parents, null).ShouldBeFalse();
        AudienceRules.Visible(EventAudience.Students, null).ShouldBeFalse();
    }

    [Test]
    public void ShouldRefuseAnAudienceThatIsNotDefined()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => AudienceRules.Reaches((EventAudience)99, UserType.Student));
    }
}
