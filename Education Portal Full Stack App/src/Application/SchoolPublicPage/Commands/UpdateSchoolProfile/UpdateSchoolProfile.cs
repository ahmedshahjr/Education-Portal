using System.Globalization;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.SchoolPublicPage.Commands.UpdateSchoolProfile;

/// <summary>
/// Saves a school's public page text. The first save creates the page (as a draft). Needs the School module. Audited
/// (SEC-LOG-03).
/// </summary>
[Authorize(Roles = Roles.Administrator, Module = ModuleCode.School)]
public record UpdateSchoolProfileCommand
{
    /// <summary>The school's public id. The master tenant must send it; a school user may leave it out.</summary>
    public Guid? SchoolId { get; init; }

    public string Tagline { get; init; } = string.Empty;

    public string About { get; init; } = string.Empty;

    /// <summary>The year the school opened, when it wants to say.</summary>
    public int? EstablishedYear { get; init; }

    public SchoolGenderPolicy GenderPolicy { get; init; } = SchoolGenderPolicy.CoEducation;

    public string? VisitingHours { get; init; }

    public string? VirtualTourUrl { get; init; }
}

public class UpdateSchoolProfileCommandValidator : AbstractValidator<UpdateSchoolProfileCommand>
{
    public UpdateSchoolProfileCommandValidator(TimeProvider timeProvider)
    {
        RuleFor(c => c.Tagline)
            .NotEmpty()
            .MaximumLength(SchoolProfile.TaglineMaxLength);

        RuleFor(c => c.About)
            .NotEmpty()
            .MaximumLength(SchoolProfile.AboutMaxLength);

        RuleFor(c => c.EstablishedYear)
            .InclusiveBetween(SchoolProfile.MinimumEstablishedYear, timeProvider.GetUtcNow().Year)
            .When(c => c.EstablishedYear.HasValue);

        RuleFor(c => c.GenderPolicy).IsInEnum();

        RuleFor(c => c.VisitingHours)
            .MaximumLength(SchoolProfile.VisitingHoursMaxLength)
            .When(c => !string.IsNullOrWhiteSpace(c.VisitingHours));

        RuleFor(c => c.VirtualTourUrl)
            .Must(url => url is not null && ContactRules.IsValidWebsite(url))
            .When(c => !string.IsNullOrWhiteSpace(c.VirtualTourUrl));
    }
}

public class UpdateSchoolProfileCommandHandler
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;
    private readonly ICurrentTenant _tenant;
    private readonly TimeProvider _timeProvider;

    public UpdateSchoolProfileCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog, ICurrentTenant tenant, TimeProvider timeProvider)
    {
        _context = context;
        _auditLog = auditLog;
        _tenant = tenant;
        _timeProvider = timeProvider;
    }

    public async Task Handle(UpdateSchoolProfileCommand request, CancellationToken cancellationToken)
    {
        var schoolId = await SchoolScope.ResolveAsync(_context, _tenant, request.SchoolId, cancellationToken);

        var profile = await _context.SchoolProfiles.FirstOrDefaultAsync(p => p.SchoolId == schoolId, cancellationToken);
        if (profile is null)
        {
            profile = SchoolProfile.Create(schoolId);
            _context.SchoolProfiles.Add(profile);
        }

        var today = DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime);
        profile.Update(
            request.Tagline,
            request.About,
            request.EstablishedYear,
            request.GenderPolicy,
            request.VisitingHours,
            request.VirtualTourUrl,
            today);

        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.PublicPageProfileChanged,
            SecurityAuditOutcome.Success,
            targetType: nameof(SchoolProfile),
            targetId: schoolId.ToString(CultureInfo.InvariantCulture));
    }
}
