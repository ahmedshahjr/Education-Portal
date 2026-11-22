using System.Globalization;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.SchoolPublicPage.Commands.SetFacility;

/// <summary>
/// Adds a facility to a school's public page, or changes its note when the school already lists that kind. A school lists
/// each kind at most once. Needs the School module. Audited (SEC-LOG-03).
/// </summary>
[Authorize(Roles = Roles.Administrator, Module = ModuleCode.School)]
public record SetFacilityCommand
{
    /// <summary>The school's public id. The master tenant must send it; a school user may leave it out.</summary>
    public Guid? SchoolId { get; init; }

    public FacilityKind Kind { get; init; }

    /// <summary>An optional note, such as "Buses cover 12 routes". A blank note is cleared.</summary>
    public string? Description { get; init; }
}

public class SetFacilityCommandValidator : AbstractValidator<SetFacilityCommand>
{
    public SetFacilityCommandValidator()
    {
        RuleFor(c => c.Kind).IsInEnum();

        RuleFor(c => c.Description)
            .MaximumLength(SchoolFacility.DescriptionMaxLength);
    }
}

public class SetFacilityCommandHandler
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;
    private readonly ICurrentTenant _tenant;

    public SetFacilityCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog, ICurrentTenant tenant)
    {
        _context = context;
        _auditLog = auditLog;
        _tenant = tenant;
    }

    public async Task Handle(SetFacilityCommand request, CancellationToken cancellationToken)
    {
        var schoolId = await SchoolScope.ResolveAsync(_context, _tenant, request.SchoolId, cancellationToken);

        var facility = await _context.SchoolFacilities.FirstOrDefaultAsync(f => f.SchoolId == schoolId && f.Kind == request.Kind, cancellationToken);
        if (facility is null)
        {
            _context.SchoolFacilities.Add(SchoolFacility.Create(schoolId, request.Kind, request.Description));
        }
        else
        {
            facility.Describe(request.Description);
        }

        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.SchoolFacilityChanged,
            SecurityAuditOutcome.Success,
            targetType: nameof(SchoolFacility),
            targetId: schoolId.ToString(CultureInfo.InvariantCulture));
    }
}
