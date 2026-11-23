using System.Globalization;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Application.SchoolPublicPage.Common;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.SchoolPublicPage.Commands.CreateClassOffering;

/// <summary>
/// Adds a class offering to a school, for the whole school or one of its campuses. Needs the School module. Audited (SEC-LOG-03).
/// </summary>
[Authorize(Roles = Roles.Administrator, Module = ModuleCode.School)]
public record CreateClassOfferingCommand
{
    /// <summary>The school's public id. The master tenant must send it; a school user may leave it out.</summary>
    public Guid? SchoolId { get; init; }

    /// <summary>The campus the offering is for, or <c>null</c> for every campus.</summary>
    public Guid CampusId { get; init; }

    public ClassLevel Level { get; init; }

    public string AcademicYear { get; init; } = string.Empty;

    public decimal MonthlyFee { get; init; }

    public decimal? AdmissionFee { get; init; }

    public string CurrencyCode { get; init; } = string.Empty;

    public int SeatsTotal { get; init; }

    public int SeatsAvailable { get; init; }

    public AdmissionStatus Status { get; init; }

    public bool ShowFee { get; init; }
}

public class CreateClassOfferingCommandValidator : AbstractValidator<CreateClassOfferingCommand>
{
    public CreateClassOfferingCommandValidator()
    {
        RuleFor(c => c.Level).IsInEnum();

        RuleFor(c => c.AcademicYear)
            .NotEmpty()
            .Matches(ClassOfferingRules.AcademicYearPattern)
            .MaximumLength(ClassOffering.AcademicYearMaxLength);

        RuleFor(c => c.MonthlyFee).GreaterThanOrEqualTo(0);

        RuleFor(c => c.AdmissionFee)
            .GreaterThanOrEqualTo(0)
            .When(c => c.AdmissionFee.HasValue);

        RuleFor(c => c.CurrencyCode)
            .NotEmpty()
            .Matches(ClassOfferingRules.CurrencyPattern);

        RuleFor(c => c.SeatsTotal).GreaterThanOrEqualTo(0);

        RuleFor(c => c.SeatsAvailable)
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(c => c.SeatsTotal);

        RuleFor(c => c.Status).IsInEnum();

        RuleFor(c => c.CampusId).NotEmpty();
    }
}

public class CreateClassOfferingCommandHandler
{
    private readonly ICampusAccess _campusAccess;
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;
    private readonly ICurrentTenant _tenant;

    public CreateClassOfferingCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog, ICurrentTenant tenant, ICampusAccess campusAccess)
    {
        _campusAccess = campusAccess;
        _context = context;
        _auditLog = auditLog;
        _tenant = tenant;
    }

    public async Task<Guid> Handle(CreateClassOfferingCommand request, CancellationToken cancellationToken)
    {
        var schoolId = await SchoolScope.ResolveAsync(_context, _tenant, request.SchoolId, cancellationToken);

        var campusId = await _context.Campuses.Where(c => c.SchoolId == schoolId).ResolveIdAsync(request.CampusId, cancellationToken);
        await _campusAccess.EnsureCampusAsync(campusId, ModuleCode.School, AccessRight.Create, cancellationToken);

        var offering = ClassOffering.Create(
            schoolId,
            campusId,
            request.Level,
            request.AcademicYear,
            request.MonthlyFee,
            request.AdmissionFee,
            request.CurrencyCode,
            request.SeatsTotal,
            request.SeatsAvailable,
            request.Status,
            request.ShowFee);

        _context.ClassOfferings.Add(offering);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.ClassOfferingChanged,
            SecurityAuditOutcome.Success,
            targetType: nameof(ClassOffering),
            targetId: offering.Id.ToString(CultureInfo.InvariantCulture));

        return offering.PublicId;
    }
}
