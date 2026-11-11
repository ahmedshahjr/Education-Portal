using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Domain.Entities;

namespace EducationPortal.Application.SchoolRequests.Commands.SubmitSchoolRequest;

/// <summary>
/// The public "request a school" form. Anyone can send it, so it is the one command without authorization. It creates a
/// pending request only: no account, no school and no domain until the master tenant approves it (AGENTS.md, section 2).
/// </summary>
public record SubmitSchoolRequestCommand : IRequest<Guid>
{
    public string SchoolName { get; init; } = string.Empty;

    public string ContactName { get; init; } = string.Empty;

    public string ContactEmail { get; init; } = string.Empty;
}

public class SubmitSchoolRequestCommandValidator : AbstractValidator<SubmitSchoolRequestCommand>
{
    public SubmitSchoolRequestCommandValidator()
    {
        RuleFor(r => r.SchoolName)
            .NotEmpty()
            .MaximumLength(SchoolRequest.SchoolNameMaxLength);

        RuleFor(r => r.ContactName)
            .NotEmpty()
            .MaximumLength(SchoolRequest.ContactNameMaxLength);

        RuleFor(r => r.ContactEmail)
            .NotEmpty()
            .MaximumLength(SchoolRequest.ContactEmailMaxLength)
            .EmailAddress();
    }

}

public class SubmitSchoolRequestCommandHandler(IApplicationDbContext context) : IRequestHandler<SubmitSchoolRequestCommand, Guid>
{
    public async Task<Guid> Handle(SubmitSchoolRequestCommand request, CancellationToken cancellationToken)
    {
        var schoolRequest = SchoolRequest.Submit(request.SchoolName, request.ContactName, request.ContactEmail);

        context.SchoolRequests.Add(schoolRequest);
        await context.SaveChangesAsync(cancellationToken);

        return schoolRequest.PublicId;
    }
}
