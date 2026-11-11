using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Domain.Entities;

namespace EducationPortal.Application.SchoolRequests.Commands.SubmitSchoolRequest;

/// <summary>
/// The public "request a school" form. Anyone can send it, so it is the one command without authorization. It creates a
/// pending request only: no account, no school and no domain until the master tenant approves it (AGENTS.md, section 2).
/// </summary>
public record SubmitSchoolRequestCommand : IRequest<int>
{
    public string SchoolName { get; init; } = string.Empty;

    /// <summary>The host name the school wants, for example <c>school1.com</c>.</summary>
    public string RequestedDomain { get; init; } = string.Empty;

    public string ContactName { get; init; } = string.Empty;

    public string ContactEmail { get; init; } = string.Empty;
}

public class SubmitSchoolRequestCommandValidator : AbstractValidator<SubmitSchoolRequestCommand>
{
    public SubmitSchoolRequestCommandValidator(IApplicationDbContext context)
    {
        RuleFor(r => r.SchoolName)
            .NotEmpty()
            .MaximumLength(SchoolRequest.SchoolNameMaxLength);

        RuleFor(r => r.RequestedDomain)
            .NotEmpty()
            .MaximumLength(School.DomainMaxLength)
            .Must(domain => School.IsValidDomain(Normalize(domain)))
            .WithMessage("The domain must be a host name such as school1.com.")
            .MustAsync(async (domain, cancellationToken) =>
                !await context.Schools.AnyAsync(s => s.Domain == Normalize(domain), cancellationToken))
            .WithMessage("This domain is already used by a school.")
            .MustAsync(async (domain, cancellationToken) =>
                !await context.SchoolRequests.AnyAsync(
                    r => r.RequestedDomain == Normalize(domain) && r.Status == SchoolRequestStatus.Pending,
                    cancellationToken))
            .WithMessage("A request for this domain is already waiting for approval.");

        RuleFor(r => r.ContactName)
            .NotEmpty()
            .MaximumLength(SchoolRequest.ContactNameMaxLength);

        RuleFor(r => r.ContactEmail)
            .NotEmpty()
            .MaximumLength(SchoolRequest.ContactEmailMaxLength)
            .EmailAddress();
    }

    private static string Normalize(string domain) => domain.Trim().TrimEnd('.').ToLowerInvariant();
}

public class SubmitSchoolRequestCommandHandler(IApplicationDbContext context) : IRequestHandler<SubmitSchoolRequestCommand, int>
{
    public async Task<int> Handle(SubmitSchoolRequestCommand request, CancellationToken cancellationToken)
    {
        var schoolRequest = SchoolRequest.Submit(request.SchoolName, request.RequestedDomain, request.ContactName, request.ContactEmail);

        context.SchoolRequests.Add(schoolRequest);
        await context.SaveChangesAsync(cancellationToken);

        return schoolRequest.Id;
    }
}
