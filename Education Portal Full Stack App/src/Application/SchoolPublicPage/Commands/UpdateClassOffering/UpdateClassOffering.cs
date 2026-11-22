using System.Globalization;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Application.SchoolPublicPage.Common;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.SchoolPublicPage.Commands.UpdateClassOffering;

/// <summary>
/// Changes a class offering's year, fees, seats, status and fee display. Its class and campus do not change: delete it and
/// create it again. Needs the School module. Audited (SEC-LOG-03).
/// </summary>
[Authorize(Roles = Roles.Administrator, Module = ModuleCode.School)]
public record UpdateClassOfferingCommand
{
    /// <summary>The school's public id. The master tenant must send it; a school user may leave it out.</summary>
    public Guid? SchoolId { get; init; }

    /// <summary>The offering's public id, taken from the route.</summary>
    public Guid OfferingId { get; init; }

    public string AcademicYear { get; init; } = string.Empty;

    public decimal MonthlyFee { get; init; }

    public decimal? AdmissionFee { get; init; }

    public string CurrencyCode { get; init; } = string.Empty;

    public int SeatsTotal { get; init; }

    public int SeatsAvailable { get; init; }

    public AdmissionStatus Status { get; init; }

    public bool ShowFee { get; init; }
}

public class UpdateClassOfferingCommandValidator : AbstractValidator<UpdateClassOfferingCommand>
{
    public UpdateClassOfferingCommandValidator()
    {
        RuleFor(c => c.OfferingId).NotEmpty();

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
    }
}

public class UpdateClassOfferingCommandHandler
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;
    private readonly ICurrentTenant _tenant;

    public UpdateClassOfferingCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog, ICurrentTenant tenant)
    {
        _context = context;
        _auditLog = auditLog;
        _tenant = tenant;
    }

    public async Task Handle(UpdateClassOfferingCommand request, CancellationToken cancellationToken)
    {
        var schoolId = await SchoolScope.ResolveAsync(_context, _tenant, request.SchoolId, cancellationToken);

        var offering = await _context.ClassOfferings.FirstOrDefaultAsync(o => o.SchoolId == schoolId && o.PublicId == request.OfferingId, cancellationToken);
        Guard.Against.NotFound(request.OfferingId, offering);

        offering.Change(
            request.AcademicYear,
            request.MonthlyFee,
            request.AdmissionFee,
            request.CurrencyCode,
            request.SeatsTotal,
            request.SeatsAvailable,
            request.Status,
            request.ShowFee);

        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.ClassOfferingChanged,
            SecurityAuditOutcome.Success,
            targetType: nameof(ClassOffering),
            targetId: offering.Id.ToString(CultureInfo.InvariantCulture));
    }
}
