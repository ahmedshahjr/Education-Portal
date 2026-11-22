using System.Globalization;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.SchoolPublicPage.Commands.RemoveFacility;

/// <summary>Removes a facility from a school's public page. Needs the School module. Audited (SEC-LOG-03).</summary>
[Authorize(Roles = Roles.Administrator, Module = ModuleCode.School)]
public record RemoveFacilityCommand
{
    /// <summary>The school's public id. The master tenant must send it; a school user may leave it out.</summary>
    public Guid? SchoolId { get; init; }

    public FacilityKind Kind { get; init; }
}

public class RemoveFacilityCommandValidator : AbstractValidator<RemoveFacilityCommand>
{
    public RemoveFacilityCommandValidator()
    {
        RuleFor(c => c.Kind).IsInEnum();
    }
}

public class RemoveFacilityCommandHandler
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;
    private readonly ICurrentTenant _tenant;

    public RemoveFacilityCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog, ICurrentTenant tenant)
    {
        _context = context;
        _auditLog = auditLog;
        _tenant = tenant;
    }

    public async Task Handle(RemoveFacilityCommand request, CancellationToken cancellationToken)
    {
        var schoolId = await SchoolScope.ResolveAsync(_context, _tenant, request.SchoolId, cancellationToken);

        var facility = await _context.SchoolFacilities.FirstOrDefaultAsync(f => f.SchoolId == schoolId && f.Kind == request.Kind, cancellationToken);
        Guard.Against.NotFound(request.Kind, facility);

        _context.SchoolFacilities.Remove(facility);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.SchoolFacilityChanged,
            SecurityAuditOutcome.Success,
            targetType: nameof(SchoolFacility),
            targetId: schoolId.ToString(CultureInfo.InvariantCulture));
    }
}
