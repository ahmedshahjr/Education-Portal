using System.Globalization;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.SchoolPublicPage.Commands.DeleteClassOffering;

/// <summary>Deletes a class offering. The row is kept (soft delete) and hidden everywhere. Needs the School module. Audited (SEC-LOG-03).</summary>
[Authorize(Roles = Roles.Administrator, Module = ModuleCode.School)]
public record DeleteClassOfferingCommand
{
    /// <summary>The school's public id. The master tenant must send it; a school user may leave it out.</summary>
    public Guid? SchoolId { get; init; }

    /// <summary>The offering's public id, taken from the route.</summary>
    public Guid OfferingId { get; init; }
}

public class DeleteClassOfferingCommandValidator : AbstractValidator<DeleteClassOfferingCommand>
{
    public DeleteClassOfferingCommandValidator()
    {
        RuleFor(c => c.OfferingId).NotEmpty();
    }
}

public class DeleteClassOfferingCommandHandler
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;
    private readonly ICurrentTenant _tenant;
    private readonly IUser _user;
    private readonly TimeProvider _timeProvider;

    public DeleteClassOfferingCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog, ICurrentTenant tenant, IUser user, TimeProvider timeProvider)
    {
        _context = context;
        _auditLog = auditLog;
        _tenant = tenant;
        _user = user;
        _timeProvider = timeProvider;
    }

    public async Task Handle(DeleteClassOfferingCommand request, CancellationToken cancellationToken)
    {
        var schoolId = await SchoolScope.ResolveAsync(_context, _tenant, request.SchoolId, cancellationToken);

        var offering = await _context.ClassOfferings.FirstOrDefaultAsync(o => o.SchoolId == schoolId && o.PublicId == request.OfferingId, cancellationToken);
        Guard.Against.NotFound(request.OfferingId, offering);

        offering.Delete(_timeProvider.GetUtcNow(), _user.Id);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.ClassOfferingChanged,
            SecurityAuditOutcome.Success,
            targetType: nameof(ClassOffering),
            targetId: offering.Id.ToString(CultureInfo.InvariantCulture));
    }
}
