using EducationPortal.Application.AddOnRequests.Queries.GetAddOnRequests;
using EducationPortal.Application.Subscriptions.Commands.AddSubscriptionModule;
using EducationPortal.Application.Subscriptions.Commands.ChangeSubscriptionPlan;
using EducationPortal.Application.Subscriptions.Commands.ExtendSubscription;
using EducationPortal.Application.Subscriptions.Commands.RemoveSubscriptionModule;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Application.UnitTests.Subscriptions;

public class SubscriptionChangeValidatorTests
{
    private static readonly Guid School = Guid.CreateVersion7();

    [TestCase(1, true)]
    [TestCase(Subscription.MaxExtensionDays, true)]
    [TestCase(0, false)]
    [TestCase(-5, false)]
    [TestCase(Subscription.MaxExtensionDays + 1, false)]
    public void ShouldAcceptOnlyOneToTheLongestExtension(int days, bool valid) =>
        new ExtendSubscriptionCommandValidator().Validate(new ExtendSubscriptionCommand { SchoolId = School, Days = days }).IsValid.ShouldBe(valid);

    [Test]
    public void ShouldRefuseAnExtensionWithoutASchool() =>
        new ExtendSubscriptionCommandValidator().Validate(new ExtendSubscriptionCommand { SchoolId = Guid.Empty, Days = 5 }).IsValid.ShouldBeFalse();

    [TestCase("STANDARD", true)]
    [TestCase("", false)]
    [TestCase("   ", false)]
    public void ShouldRequireAPlanCode(string code, bool valid) =>
        new ChangeSubscriptionPlanCommandValidator().Validate(new ChangeSubscriptionPlanCommand { SchoolId = School, PlanCode = code }).IsValid.ShouldBe(valid);

    [Test]
    public void ShouldRefuseAPlanCodeThatIsTooLong() =>
        new ChangeSubscriptionPlanCommandValidator()
            .Validate(new ChangeSubscriptionPlanCommand { SchoolId = School, PlanCode = new string('A', Plan.CodeMaxLength + 1) }).IsValid.ShouldBeFalse();

    [TestCase("FEES", true)]
    [TestCase("", false)]
    public void ShouldRequireAModuleCodeToAdd(string code, bool valid) =>
        new AddSubscriptionModuleCommandValidator().Validate(new AddSubscriptionModuleCommand { SchoolId = School, ModuleCode = code }).IsValid.ShouldBe(valid);

    [TestCase("FEES", true)]
    [TestCase("", false)]
    public void ShouldRequireAModuleCodeToRemove(string code, bool valid) =>
        new RemoveSubscriptionModuleCommandValidator().Validate(new RemoveSubscriptionModuleCommand { SchoolId = School, ModuleCode = code }).IsValid.ShouldBe(valid);

    [Test]
    public void ShouldRefuseAModuleCodeThatIsTooLong()
    {
        var code = new string('A', PortalModule.CodeMaxLength + 1);

        new AddSubscriptionModuleCommandValidator().Validate(new AddSubscriptionModuleCommand { SchoolId = School, ModuleCode = code }).IsValid.ShouldBeFalse();
        new RemoveSubscriptionModuleCommandValidator().Validate(new RemoveSubscriptionModuleCommand { SchoolId = School, ModuleCode = code }).IsValid.ShouldBeFalse();
    }

    [Test]
    public void ShouldAcceptNoStatusOrAKnownStatusForTheRequestList()
    {
        var validator = new GetAddOnRequestsQueryValidator();

        validator.Validate(new GetAddOnRequestsQuery()).IsValid.ShouldBeTrue();
        validator.Validate(new GetAddOnRequestsQuery(AddOnRequestStatus.Pending)).IsValid.ShouldBeTrue();
        validator.Validate(new GetAddOnRequestsQuery((AddOnRequestStatus)99)).IsValid.ShouldBeFalse();
    }
}
