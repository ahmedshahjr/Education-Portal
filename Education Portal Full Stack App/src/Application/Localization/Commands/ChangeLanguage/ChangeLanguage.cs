using EducationPortal.Application.Common.Interfaces;

namespace EducationPortal.Application.Localization.Commands.ChangeLanguage;

/// <summary>
/// Checks that the host's school offers a language, so the visitor can switch to it. A signed-in user's choice is also
/// saved on their account, so it follows them to other devices; remembering it in this browser (a cookie) is the web
/// layer's job. Anyone may switch, before or after sign-in; only the school's own languages are accepted.
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
    private readonly IUser _user;
    private readonly IIdentityService _identityService;

    public ChangeLanguageCommandHandler(IUser user, IIdentityService identityService)
    {
        _user = user;
        _identityService = identityService;
    }

    public async Task<string> Handle(ChangeLanguageCommand request, CancellationToken cancellationToken)
    {
        var language = request.Language.ToLowerInvariant();

        if (_user.Id is { } userId)
        {
            var saved = await _identityService.SetPreferredLanguageAsync(userId, language);

            if (!saved.Succeeded)
            {
                // The session's user no longer exists; the session check rejects the next request.
                throw new UnauthorizedAccessException();
            }
        }

        return language;
    }
}
