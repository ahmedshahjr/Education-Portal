using System.Globalization;
using EducationPortal.Application.AcademicCalendar.Common;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.AcademicCalendar.Commands.CreateAcademicYear;

/// <summary>
/// Adds an academic year to a school, planned until it is changed. Its name is unique in the school and its dates do not
/// overlap another year. Needs the Academic setup module. Audited (SEC-LOG-03).
/// </summary>
[Authorize(Roles = Roles.Administrator, Module = ModuleCode.AcademicSetup)]
public record CreateAcademicYearCommand : IRequest<Guid>
{
    /// <summary>The school's public id. The master tenant must send it; a school user may leave it out.</summary>
    public Guid? SchoolId { get; init; }

    public string Name { get; init; } = string.Empty;

    public DateOnly StartDate { get; init; }

    public DateOnly EndDate { get; init; }

    /// <summary>When true the new year becomes the school's current year.</summary>
    public bool IsCurrent { get; init; }
}

public class CreateAcademicYearCommandValidator : AbstractValidator<CreateAcademicYearCommand>
{
    public CreateAcademicYearCommandValidator()
    {
        RuleFor(c => c.Name).NotEmpty().MaximumLength(AcademicYear.NameMaxLength);
        AcademicYearValidation.Dates(this, c => c.StartDate, c => c.EndDate);
    }
}

public class CreateAcademicYearCommandHandler : IRequestHandler<CreateAcademicYearCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;
    private readonly ICurrentTenant _tenant;

    public CreateAcademicYearCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog, ICurrentTenant tenant)
    {
        _context = context;
        _auditLog = auditLog;
        _tenant = tenant;
    }

    public async Task<Guid> Handle(CreateAcademicYearCommand request, CancellationToken cancellationToken)
    {
        var schoolId = await SchoolScope.ResolveAsync(_context, _tenant, request.SchoolId, cancellationToken);
        await AcademicYearChecks.EnsureFitsAsync(_context, schoolId, null, request.Name, request.StartDate, request.EndDate, cancellationToken);

        var year = AcademicYear.Create(schoolId, request.Name, request.StartDate, request.EndDate);
        _context.AcademicYears.Add(year);

        await _context.ExecuteInTransactionAsync(
            async ct =>
            {
                await _context.SaveChangesAsync(ct);
                if (request.IsCurrent)
                {
                    await AcademicYearChecks.MakeCurrentAsync(_context, year, ct);
                }

                return true;
            },
            cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.AcademicYearChanged,
            SecurityAuditOutcome.Success,
            targetType: nameof(AcademicYear),
            targetId: year.Id.ToString(CultureInfo.InvariantCulture));

        return year.PublicId;
    }
}
