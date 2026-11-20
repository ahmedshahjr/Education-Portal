using System.Globalization;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.SchoolPublicPage.Commands.UnpublishSchoolProfile;

/// <summary>Hides a school's public page from the public. The text is kept. Needs the School module. Audited (SEC-LOG-03).</summary>
[Authorize(Roles = Roles.Administrator, Module = ModuleCode.School)]
public record UnpublishSchoolProfileCommand : IRequest
{
    /// <summary>The school's public id. The master tenant must send it; a school user may leave it out.</summary>
    public Guid? SchoolId { get; init; }
}

public class UnpublishSchoolProfileCommandHandler : IRequestHandler<UnpublishSchoolProfileCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;
    private readonly ICurrentTenant _tenant;

    public UnpublishSchoolProfileCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog, ICurrentTenant tenant)
    {
        _context = context;
        _auditLog = auditLog;
        _tenant = tenant;
    }

    public async Task Handle(UnpublishSchoolProfileCommand request, CancellationToken cancellationToken)
    {
        var schoolId = await SchoolScope.ResolveAsync(_context, _tenant, request.SchoolId, cancellationToken);

        var profile = await _context.SchoolProfiles.FirstOrDefaultAsync(p => p.SchoolId == schoolId, cancellationToken);
        Guard.Against.NotFound(schoolId, profile);

        profile.Unpublish();
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.PublicPageUnpublished,
            SecurityAuditOutcome.Success,
            targetType: nameof(SchoolProfile),
            targetId: schoolId.ToString(CultureInfo.InvariantCulture));
    }
}
