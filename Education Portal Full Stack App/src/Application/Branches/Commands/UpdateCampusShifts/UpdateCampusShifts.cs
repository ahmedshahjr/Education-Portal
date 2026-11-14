using System.Globalization;
using EducationPortal.Application.Common.Exceptions;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;

namespace EducationPortal.Application.Branches.Commands.UpdateCampusShifts;

/// <summary>
/// Replaces the shifts of a campus of the current school (for example Morning and Evening), each with its start and end
/// time in the school's country time zone. At most four shifts, with distinct names; a shift ends on the day it starts. The
/// shifts it had are kept for audit but no longer listed. Needs the Branches module. Audited (SEC-LOG-03).
/// </summary>
[Authorize(Roles = Roles.Administrator, Module = ModuleCodes.Branch)]
public record UpdateCampusShiftsCommand : IRequest
{
    /// <summary>The campus's public id, taken from the route (ENG-ID-02).</summary>
    public Guid BranchId { get; init; }

    public IReadOnlyList<CampusShiftInput> Shifts { get; init; } = [];
}

/// <summary>One shift of a campus: its name and its start and end time.</summary>
public sealed record CampusShiftInput(string Name, TimeOnly StartTime, TimeOnly EndTime);

public class UpdateCampusShiftsCommandValidator : AbstractValidator<UpdateCampusShiftsCommand>
{
    public UpdateCampusShiftsCommandValidator()
    {
        RuleFor(c => c.BranchId).NotEmpty();

        RuleFor(c => c.Shifts)
            .Must(shifts => shifts.Count <= Branch.MaxShiftsPerCampus)
            .WithMessage(_ => EducationPortal.Application.Common.Localization.ValidationMessages.CampusShiftLimit);

        RuleFor(c => c.Shifts)
            .Must(shifts => shifts.Select(s => (s.Name ?? string.Empty).Trim()).Distinct(StringComparer.OrdinalIgnoreCase).Count() == shifts.Count)
            .WithMessage(_ => EducationPortal.Application.Common.Localization.ValidationMessages.CampusShiftNamesDistinct);

        RuleForEach(c => c.Shifts).ChildRules(shift =>
        {
            shift.RuleFor(s => s.Name)
                .NotEmpty()
                .WithMessage(_ => EducationPortal.Application.Common.Localization.ValidationMessages.CampusShiftNameRequired);

            shift.RuleFor(s => s.Name)
                .MaximumLength(CampusShift.NameMaxLength)
                .WithMessage(_ => EducationPortal.Application.Common.Localization.ValidationMessages.CampusShiftNameTooLong);

            shift.RuleFor(s => s.EndTime)
                .GreaterThan(s => s.StartTime)
                .WithMessage(_ => EducationPortal.Application.Common.Localization.ValidationMessages.CampusShiftEndAfterStart);
        });
    }
}

public class UpdateCampusShiftsCommandHandler : IRequestHandler<UpdateCampusShiftsCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;
    private readonly TimeProvider _timeProvider;
    private readonly ICurrentTenant _tenant;
    private readonly IUser _user;

    public UpdateCampusShiftsCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog, TimeProvider timeProvider, ICurrentTenant tenant, IUser user)
    {
        _context = context;
        _auditLog = auditLog;
        _timeProvider = timeProvider;
        _tenant = tenant;
        _user = user;
    }

    public async Task Handle(UpdateCampusShiftsCommand request, CancellationToken cancellationToken)
    {
        var schoolId = _tenant.SchoolId ?? throw new ForbiddenAccessException();

        // Include loads the campus's active shifts (the query filter hides the replaced ones).
        var branch = await _context.Branches
            .Include(b => b.Shifts)
            .FirstOrDefaultAsync(b => b.SchoolId == schoolId && b.PublicId == request.BranchId, cancellationToken);
        Guard.Against.NotFound(request.BranchId, branch);

        branch.ReplaceShifts(
            request.Shifts.Select(s => (s.Name, s.StartTime, s.EndTime)).ToList(),
            _timeProvider.GetUtcNow(),
            _user.Id);

        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.BranchUpdated,
            SecurityAuditOutcome.Success,
            targetType: nameof(Branch),
            targetId: branch.Id.ToString(CultureInfo.InvariantCulture));
    }
}
