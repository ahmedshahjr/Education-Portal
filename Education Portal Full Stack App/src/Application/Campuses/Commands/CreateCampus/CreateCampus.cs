using System.Globalization;
using EducationPortal.Application.Campuses.Common;
using EducationPortal.Application.Common.Exceptions;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Localization;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;
using FluentValidation.Results;

namespace EducationPortal.Application.Campuses.Commands.CreateCampus;

/// <summary>
/// Adds a campus to the current school. The campus gets a four-digit code, generated here and unique within the school. The school
/// is the one the request's host resolves to (SEC-AUTHZ-04). Needs the Campuses module. Audited (SEC-LOG-03).
/// </summary>
[Authorize(Roles = Roles.Administrator, Module = ModuleCode.Campus)]
public record CreateCampusCommand
{
    public string Name { get; init; } = string.Empty;

    /// <summary>The name in the local language and script, or blank for none.</summary>
    public string? LocalName { get; init; }

    /// <summary>Whether the campus teaches boys, girls or both.</summary>
    public SchoolGenderPolicy GenderPolicy { get; init; } = SchoolGenderPolicy.CoEducation;

    /// <summary>The contact and registration details of the campus. Required: see <see cref="CampusDetailsInput"/>.</summary>
    public CampusDetailsInput Details { get; init; } = new();
}

public class CreateCampusCommandValidator : AbstractValidator<CreateCampusCommand>
{
    public CreateCampusCommandValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty()
            .MaximumLength(Campus.NameMaxLength);

        RuleFor(c => c.LocalName).MaximumLength(Campus.LocalNameMaxLength);
        RuleFor(c => c.GenderPolicy).IsInEnum();

        RuleFor(c => c.Details)
            .SetValidator(new CampusDetailsInputValidator());
    }
}

public class CreateCampusCommandHandler
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;
    private readonly ICurrentTenant _tenant;

    public CreateCampusCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog, ICurrentTenant tenant)
    {
        _context = context;
        _auditLog = auditLog;
        _tenant = tenant;
    }

    public async Task<Guid> Handle(CreateCampusCommand request, CancellationToken cancellationToken)
    {
        var schoolId = _tenant.SchoolId;

        var used = (await _context.Campuses
                .Where(b => b.SchoolId == schoolId)
                .Select(b => b.Code)
                .ToListAsync(cancellationToken))
            .ToHashSet(StringComparer.Ordinal);

        var code = CampusCodes.NextFree(used, Random.Shared)
            ?? throw new Application.Common.Exceptions.ValidationException([new ValidationFailure(nameof(request.Name),ValidationMessages.CampusCodesExhausted)]);

        var details = await CampusDetailsResolver.ResolveAsync(_context, schoolId, request.Details, cancellationToken);

        var campus = Campus.Create(schoolId, request.Name, code);
        campus.UpdateDetails(details);
        campus.SetProfile(request.LocalName, request.GenderPolicy);
        _context.Campuses.Add(campus);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.CampusCreated,
            SecurityAuditOutcome.Success,
            targetType: nameof(Campus),
            targetId: campus.Id.ToString(CultureInfo.InvariantCulture));

        return campus.PublicId;
    }
}
