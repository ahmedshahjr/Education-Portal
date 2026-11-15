using System.Globalization;
using EducationPortal.Application.Common.Exceptions;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;
using FluentValidation.Results;

namespace EducationPortal.Application.Branches.Commands.UpdateCampusSettings;

/// <summary>
/// Sets whether the current school's campus codes show a prefix, and the prefix (1 to 6 letters or digits, required when the
/// prefix is on). The school is the one the request's host resolves to (SEC-AUTHZ-04). Needs the Branches module.
/// Audited (SEC-LOG-03).
/// </summary>
[Authorize(Roles = Roles.Administrator, Module = ModuleCodes.Branch)]
public record UpdateCampusSettingsCommand : IRequest
{
    public bool PrefixEnabled { get; init; }

    public string? Prefix { get; init; }
}

public class UpdateCampusSettingsCommandValidator : AbstractValidator<UpdateCampusSettingsCommand>
{
    public UpdateCampusSettingsCommandValidator()
    {
        RuleFor(c => c.Prefix)
            .NotEmpty()
            .When(c => c.PrefixEnabled)
            .WithMessage(_ => EducationPortal.Application.Common.Localization.ValidationMessages.CampusPrefixRequired);

        RuleFor(c => c.Prefix)
            .MaximumLength(School.CampusCodePrefixMaxLength)
            .Matches("^[A-Za-z0-9]*$")
            .When(c => !string.IsNullOrWhiteSpace(c.Prefix))
            .WithMessage(_ => EducationPortal.Application.Common.Localization.ValidationMessages.CampusPrefixFormat);
    }
}

public class UpdateCampusSettingsCommandHandler : IRequestHandler<UpdateCampusSettingsCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;
    private readonly ICurrentTenant _tenant;

    public UpdateCampusSettingsCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog, ICurrentTenant tenant)
    {
        _context = context;
        _auditLog = auditLog;
        _tenant = tenant;
    }

    public async Task Handle(UpdateCampusSettingsCommand request, CancellationToken cancellationToken)
    {
        var schoolId = _tenant.SchoolId ?? throw new ForbiddenAccessException();
        var school = await _context.Schools.FirstOrDefaultAsync(s => s.Id == schoolId, cancellationToken);
        Guard.Against.NotFound(schoolId, school);

        school.ConfigureCampusCodes(request.PrefixEnabled, request.Prefix);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.CampusSettingsChanged,
            SecurityAuditOutcome.Success,
            targetType: nameof(School),
            targetId: school.Id.ToString(CultureInfo.InvariantCulture));
    }
}
