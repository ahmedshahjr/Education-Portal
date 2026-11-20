using System.Globalization;
using EducationPortal.Application.AcademicCalendar.Common;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.AcademicCalendar.Commands.DeleteTerm;

/// <summary>
/// Deletes a term of a year that is not closed. The row is kept (soft delete) and hidden everywhere. Needs the Academic
/// setup module. Audited (SEC-LOG-03).
/// </summary>
[Authorize(Roles = Roles.Administrator, Module = ModuleCode.AcademicSetup)]
public record DeleteTermCommand : IRequest
{
    /// <summary>The school's public id. The master tenant must send it; a school user may leave it out.</summary>
    public Guid? SchoolId { get; init; }

    /// <summary>The term's public id, taken from the route.</summary>
    public Guid TermId { get; init; }
}

public class DeleteTermCommandValidator : AbstractValidator<DeleteTermCommand>
{
    public DeleteTermCommandValidator()
    {
        RuleFor(c => c.TermId).NotEmpty();
    }
}

public class DeleteTermCommandHandler : IRequestHandler<DeleteTermCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;
    private readonly ICurrentTenant _tenant;
    private readonly IUser _user;
    private readonly TimeProvider _timeProvider;

    public DeleteTermCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog, ICurrentTenant tenant, IUser user, TimeProvider timeProvider)
    {
        _context = context;
        _auditLog = auditLog;
        _tenant = tenant;
        _user = user;
        _timeProvider = timeProvider;
    }

    public async Task Handle(DeleteTermCommand request, CancellationToken cancellationToken)
    {
        var schoolId = await SchoolScope.ResolveAsync(_context, _tenant, request.SchoolId, cancellationToken);

        var term = await _context.Terms.FirstOrDefaultAsync(t => t.SchoolId == schoolId && t.PublicId == request.TermId, cancellationToken);
        Guard.Against.NotFound(request.TermId, term);

        var year = await _context.AcademicYears.FirstAsync(y => y.Id == term.AcademicYearId, cancellationToken);
        AcademicYearChecks.EnsureOpen(year);

        term.Delete(_timeProvider.GetUtcNow(), _user.Id);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.AcademicYearChanged,
            SecurityAuditOutcome.Success,
            targetType: nameof(Term),
            targetId: term.Id.ToString(CultureInfo.InvariantCulture));
    }
}
