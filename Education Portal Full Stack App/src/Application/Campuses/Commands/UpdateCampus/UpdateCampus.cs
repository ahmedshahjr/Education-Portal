using System.Globalization;
using EducationPortal.Application.Campuses.Common;
using EducationPortal.Application.Common.Exceptions;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;

namespace EducationPortal.Application.Campuses.Commands.UpdateCampus;

/// <summary>
/// Renames a campus of the current school. Its code is generated once and does not change. Needs the Campuses module.
/// Audited (SEC-LOG-03).
/// </summary>
[Authorize(Roles = Roles.Administrator, Module = ModuleCode.Campus)]
public record UpdateCampusCommand : IRequest
{
    /// <summary>The campus's public id, taken from the route (ENG-ID-02).</summary>
    public Guid CampusId { get; init; }

    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// The campus's contact and registration details. When it is left out the details stay as they are; when it is sent,
    /// it replaces them, so an empty object clears them.
    /// </summary>
    public CampusDetailsInput? Details { get; init; }
}

public class UpdateCampusCommandValidator : AbstractValidator<UpdateCampusCommand>
{
    public UpdateCampusCommandValidator()
    {
        RuleFor(c => c.CampusId).NotEmpty();

        RuleFor(c => c.Name)
            .NotEmpty()
            .MaximumLength(Campus.NameMaxLength);

        RuleFor(c => c.Details!)
            .SetValidator(new CampusDetailsInputValidator())
            .When(c => c.Details is not null);
    }
}

public class UpdateCampusCommandHandler : IRequestHandler<UpdateCampusCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;
    private readonly ICurrentTenant _tenant;

    public UpdateCampusCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog, ICurrentTenant tenant)
    {
        _context = context;
        _auditLog = auditLog;
        _tenant = tenant;
    }

    public async Task Handle(UpdateCampusCommand request, CancellationToken cancellationToken)
    {
        var schoolId = _tenant.SchoolId ?? throw new ForbiddenAccessException();
        var campus = await _context.Campuses.FirstOrDefaultAsync(b => b.SchoolId == schoolId && b.PublicId == request.CampusId, cancellationToken);
        Guard.Against.NotFound(request.CampusId, campus);

        if (request.Details is not null)
        {
            var details = await CampusDetailsResolver.ResolveAsync(_context, schoolId, request.Details, cancellationToken);
            campus.UpdateDetails(details);
        }

        campus.Rename(request.Name);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.CampusUpdated,
            SecurityAuditOutcome.Success,
            targetType: nameof(Campus),
            targetId: campus.Id.ToString(CultureInfo.InvariantCulture));
    }
}
