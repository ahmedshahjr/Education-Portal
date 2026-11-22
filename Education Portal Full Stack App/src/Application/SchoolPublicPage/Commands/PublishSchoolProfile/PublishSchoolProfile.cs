using System.Globalization;
using EducationPortal.Application.Common.Exceptions;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using FluentValidation.Results;

namespace EducationPortal.Application.SchoolPublicPage.Commands.PublishSchoolProfile;

/// <summary>
/// Shows a school's public page to the public. The page needs a tagline and an about text first. Needs the School module.
/// Audited (SEC-LOG-03).
/// </summary>
[Authorize(Roles = Roles.Administrator, Module = ModuleCode.School)]
public record PublishSchoolProfileCommand
{
    /// <summary>The school's public id. The master tenant must send it; a school user may leave it out.</summary>
    public Guid? SchoolId { get; init; }
}

public class PublishSchoolProfileCommandHandler
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;
    private readonly ICurrentTenant _tenant;
    private readonly TimeProvider _timeProvider;

    public PublishSchoolProfileCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog, ICurrentTenant tenant, TimeProvider timeProvider)
    {
        _context = context;
        _auditLog = auditLog;
        _tenant = tenant;
        _timeProvider = timeProvider;
    }

    public async Task Handle(PublishSchoolProfileCommand request, CancellationToken cancellationToken)
    {
        var schoolId = await SchoolScope.ResolveAsync(_context, _tenant, request.SchoolId, cancellationToken);

        var profile = await _context.SchoolProfiles.FirstOrDefaultAsync(p => p.SchoolId == schoolId, cancellationToken);
        Guard.Against.NotFound(schoolId, profile);

        try
        {
            profile.Publish(_timeProvider.GetUtcNow());
        }
        catch (InvalidOperationException ex)
        {
            // The entity refuses an empty page; report it as a validation failure, not a server error.
            throw new EducationPortal.Application.Common.Exceptions.ValidationException([new ValidationFailure(nameof(SchoolProfile), ex.Message)]);
        }

        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.PublicPagePublished,
            SecurityAuditOutcome.Success,
            targetType: nameof(SchoolProfile),
            targetId: schoolId.ToString(CultureInfo.InvariantCulture));
    }
}
