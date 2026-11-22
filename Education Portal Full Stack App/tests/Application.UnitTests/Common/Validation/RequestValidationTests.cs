using EducationPortal.Application.Common.Exceptions;
using EducationPortal.Application.Common.Validation;
using FluentValidation;
using ValidationException = EducationPortal.Application.Common.Exceptions.ValidationException;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Application.UnitTests.Common.Validation;

public sealed record NameRequest(string Name);

public sealed class NameRequestValidator : AbstractValidator<NameRequest>
{
    public NameRequestValidator()
    {
        RuleFor(r => r.Name).NotEmpty().MaximumLength(5);
    }
}

public sealed class NameRequestStartsWithValidator : AbstractValidator<NameRequest>
{
    public NameRequestStartsWithValidator()
    {
        RuleFor(r => r.Name).Must(n => n.StartsWith('A')).WithMessage("Must start with A.");
    }
}

/// <summary>
/// RequestValidation runs every validator registered for the request (called by RequestRules) and refuses it with one ValidationException that
/// lists all failures. A valid request passes, an invalid one is refused, and the length limit is checked at its edge.
/// </summary>
public class RequestValidationTests
{
    [Test]
    public async Task ShouldPassAValidRequest()
    {
        await Should.NotThrowAsync(() =>
            RequestValidation.ValidateAsync([new NameRequestValidator()], new NameRequest("Ali"), CancellationToken.None));
    }

    [Test]
    public async Task ShouldRefuseAnInvalidRequestWithTheFailuresByProperty()
    {
        var thrown = await Should.ThrowAsync<ValidationException>(() =>
            RequestValidation.ValidateAsync([new NameRequestValidator()], new NameRequest(string.Empty), CancellationToken.None));

        thrown.Errors.ShouldContainKey(nameof(NameRequest.Name));
        thrown.Errors[nameof(NameRequest.Name)].ShouldNotBeEmpty();
    }

    [Test]
    public async Task ShouldAcceptANameOfExactlyTheMaximumLength()
    {
        await Should.NotThrowAsync(() =>
            RequestValidation.ValidateAsync([new NameRequestValidator()], new NameRequest("ABCDE"), CancellationToken.None));
    }

    [Test]
    public async Task ShouldRefuseANameOneCharacterOverTheMaximumLength()
    {
        await Should.ThrowAsync<ValidationException>(() =>
            RequestValidation.ValidateAsync([new NameRequestValidator()], new NameRequest("ABCDEF"), CancellationToken.None));
    }

    [Test]
    public async Task ShouldCollectFailuresFromEveryValidator()
    {
        var thrown = await Should.ThrowAsync<ValidationException>(() =>
            RequestValidation.ValidateAsync(
                [new NameRequestValidator(), new NameRequestStartsWithValidator()],
                new NameRequest("Bobbbbbb"),
                CancellationToken.None));

        thrown.Errors[nameof(NameRequest.Name)].ShouldContain("Must start with A.");
        thrown.Errors[nameof(NameRequest.Name)].Length.ShouldBe(2);
    }

    [Test]
    public async Task ShouldPassWhenNoValidatorIsRegistered()
    {
        await Should.NotThrowAsync(() =>
            RequestValidation.ValidateAsync([], new NameRequest(string.Empty), CancellationToken.None));
    }
}
