using EducationPortal.Application.Common.Interfaces;

namespace EducationPortal.Application.Localization.Commands.ChangeLanguage;

/// <summary>
/// Checks that the host's school offers a language, so the visitor can switch to it. Remembering the choice (a cookie)
/// is the web layer's job. Anyone may switch, before or after sign-in; only the school's own languages are accepted.
/// </summary>
/// <param name="Language">An ISO 639-1 code, for example <c>ar</c>.</param>
public record ChangeLanguageCommand(string Language) : IRequest<string>;

public class ChangeLanguageCommandValidator : AbstractValidator<ChangeLanguageCommand>
{
    public ChangeLanguageCommandValidator(ITenantLocaleSettings settings)
    {
        RuleFor(c => c.Language)
            .NotEmpty()
            .MaximumLength(Domain.Localization.Language.CodeLength)
            .MustAsync(async (language, cancellationToken) => (await settings.GetAsync(cancellationToken)).Offers(language))
            .WithMessage(_ => ValidationMessages.LanguageNotOffered);
    }
}

public class ChangeLanguageCommandHandler : IRequestHandler<ChangeLanguageCommand, string>
{
    public Task<string> Handle(ChangeLanguageCommand request, CancellationToken cancellationToken) =>
        Task.FromResult(request.Language.ToLowerInvariant());
}
