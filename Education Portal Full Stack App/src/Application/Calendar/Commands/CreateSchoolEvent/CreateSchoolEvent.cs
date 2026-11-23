using EducationPortal.Application.Common.Exceptions;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Localization;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Communication;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.Calendar.Commands.CreateSchoolEvent;

/// <summary>
/// Adds an event to the school calendar and a notification for each account it reaches, in one save. Needs the School module
/// and the Create right. Audited.
/// </summary>
[Authorize(Module = ModuleCode.School, Right = AccessRight.Create)]
public record CreateSchoolEventCommand
{
    public string Title { get; init; } = string.Empty;

    public string? Description { get; init; }

    public DateTimeOffset StartsAt { get; init; }

    public DateTimeOffset EndsAt { get; init; }

    public EventAudience Audience { get; init; }
}

public class CreateSchoolEventCommandValidator : AbstractValidator<CreateSchoolEventCommand>
{
    public CreateSchoolEventCommandValidator()
    {
        RuleFor(c => c.Title).NotEmpty().MaximumLength(SchoolEvent.TitleMaxLength);
        RuleFor(c => c.Description).MaximumLength(SchoolEvent.DescriptionMaxLength);
        RuleFor(c => c.EndsAt).GreaterThan(c => c.StartsAt).WithMessage(_ => ValidationMessages.EventEndsAfterStart);
        RuleFor(c => c.Audience).IsInEnum();
    }
}

public class CreateSchoolEventCommandHandler
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenant _tenant;
    private readonly IIdentityService _identityService;

    public CreateSchoolEventCommandHandler(IApplicationDbContext context, ICurrentTenant tenant, IIdentityService identityService)
    {
        _context = context;
        _tenant = tenant;
        _identityService = identityService;
    }

    public async Task<Guid> Handle(CreateSchoolEventCommand request, CancellationToken cancellationToken)
    {
        var schoolId = _tenant.SchoolId;

        var schoolEvent = SchoolEvent.Create(schoolId, request.Title, request.Description, request.StartsAt, request.EndsAt, request.Audience);
        _context.SchoolEvents.Add(schoolEvent);

        // Notifications are added to the same save, so the event and its notifications are stored together or not at all.
        foreach (var account in await RecipientsAsync(schoolId, request.Audience, cancellationToken))
        {
            _context.Notifications.Add(Notification.For(schoolEvent, account));
        }

        await _context.SaveChangesAsync(cancellationToken);

        return schoolEvent.PublicId;
    }

    private async Task<IReadOnlyList<string>> RecipientsAsync(int schoolId, EventAudience audience, CancellationToken cancellationToken)
    {
        if (audience == EventAudience.All)
        {
            return await _identityService.FindAccountIdsAsync(schoolId, null);
        }

        var people = await _context.UserDetails.AsNoTracking()
            .Select(u => new { u.Id, u.UserType })
            .ToListAsync(cancellationToken);

        var personIds = people
            .Where(p => AudienceRules.Reaches(audience, p.UserType))
            .Select(p => p.Id)
            .ToList();

        return await _identityService.FindAccountIdsAsync(schoolId, personIds);
    }
}
