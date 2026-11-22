using EducationPortal.Application.Common.Exceptions;
using EducationPortal.Application.Common.Security;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using NUnit.Framework;
using Shouldly;
using ValidationException = EducationPortal.Application.Common.Exceptions.ValidationException;

namespace EducationPortal.Application.UnitTests.Common.Security;

public sealed record RulesRequest(string Name);

/// <summary>
/// RequestRules is the explicit check each controller action calls before its use case (ADR-025). The authorisation of the
/// request type runs first, then its validators, and a request that is refused by the authorisation never reaches them.
/// </summary>
public class RequestRulesTests
{
    [Test]
    public async Task ShouldRunTheAuthorizerBeforeTheValidators()
    {
        var authorizer = new Mock<IRequestAuthorizer>();
        authorizer
            .Setup(a => a.AuthorizeAsync(It.IsAny<Type>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromException(new ForbiddenAccessException()));
        var validator = ValidatorWith();
        var rules = new RequestRules(authorizer.Object, new RegisteredValidators(validator.Object));

        await Should.ThrowAsync<ForbiddenAccessException>(() =>
            rules.CheckAsync(new RulesRequest("Ali"), CancellationToken.None));

        validator.Verify(v => v.ValidateAsync(It.IsAny<IValidationContext>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ShouldRefuseAnInvalidRequestWithEveryFailure()
    {
        var first = ValidatorWith(
            new ValidationFailure(nameof(RulesRequest.Name), "Name is required."),
            new ValidationFailure("Age", "Age must be positive."));
        var second = ValidatorWith(new ValidationFailure(nameof(RulesRequest.Name), "Name is too long."));
        var rules = new RequestRules(AllowingAuthorizer().Object, new RegisteredValidators(first.Object, second.Object));

        var thrown = await Should.ThrowAsync<ValidationException>(() =>
            rules.CheckAsync(new RulesRequest(string.Empty), CancellationToken.None));

        thrown.Errors.Count.ShouldBe(2);
        thrown.Errors[nameof(RulesRequest.Name)].ShouldBe(new[] { "Name is required.", "Name is too long." });
        thrown.Errors["Age"].ShouldBe(new[] { "Age must be positive." });
    }

    [Test]
    public async Task ShouldPassAValidRequestWithNoValidators()
    {
        var authorizer = AllowingAuthorizer();
        var rules = new RequestRules(authorizer.Object, new RegisteredValidators());

        await Should.NotThrowAsync(() => rules.CheckAsync(new RulesRequest("Ali"), CancellationToken.None));

        authorizer.Verify(a => a.AuthorizeAsync(typeof(RulesRequest), It.IsAny<CancellationToken>()), Times.Once);
    }

    private static Mock<IRequestAuthorizer> AllowingAuthorizer()
    {
        var authorizer = new Mock<IRequestAuthorizer>();
        authorizer
            .Setup(a => a.AuthorizeAsync(It.IsAny<Type>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return authorizer;
    }

    private static Mock<IValidator<RulesRequest>> ValidatorWith(params ValidationFailure[] failures)
    {
        var validator = new Mock<IValidator<RulesRequest>>();
        validator
            .Setup(v => v.ValidateAsync(It.IsAny<IValidationContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(failures));
        return validator;
    }

    /// <summary>
    /// Hands RequestRules the validators registered for <see cref="RulesRequest"/>, the way the dependency container does.
    /// </summary>
    private sealed class RegisteredValidators(params IValidator<RulesRequest>[] validators) : IServiceProvider
    {
        public object? GetService(Type serviceType) =>
            serviceType == typeof(IEnumerable<IValidator<RulesRequest>>) ? validators.ToList() : null;
    }
}
