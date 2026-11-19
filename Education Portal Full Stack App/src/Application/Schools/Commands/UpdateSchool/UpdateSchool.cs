using System.Globalization;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;

namespace EducationPortal.Application.Schools.Commands.UpdateSchool;

/// <summary>
/// Changes a school's name and the domain it is reached on. The domain must be a host name no other school uses, and the
/// master tenant's domain never changes. A master-tenant use case, so the school id may come from the route
/// (SEC-AUTHZ-04, SEC-AUTHZ-07). Audited (SEC-LOG-03).
/// </summary>
[Authorize(Roles = Roles.Administrator, MasterTenant = true)]
public record UpdateSchoolCommand : IRequest
{
    /// <summary>The school's public id (ENG-ID-02).</summary>
    public Guid SchoolId { get; init; }

    public string Name { get; init; } = string.Empty;

    /// <summary>The host name the school is reached on, for example <c>school1.com</c>. Stored in lower case.</summary>
    public string Domain { get; init; } = string.Empty;
}

public class UpdateSchoolCommandValidator : AbstractValidator<UpdateSchoolCommand>
{
    public UpdateSchoolCommandValidator(IApplicationDbContext context)
    {
        RuleFor(c => c.SchoolId).NotEmpty();

        RuleFor(c => c.Name)
            .NotEmpty()
            .MaximumLength(School.NameMaxLength);

        // Optional: a blank domain removes it, and the school then signs in on the master tenant's domain.
        RuleFor(c => c.Domain)
            .Must(domain => School.IsValidDomain(Normalize(domain)))
            .WithMessage(_ => ValidationMessages.SchoolDomainFormat)
            .When(c => !string.IsNullOrWhiteSpace(c.Domain))
            .DependentRules(() =>
                RuleFor(c => c.Domain)
                    .MustAsync(async (command, domain, cancellationToken) =>
                        !await context.Schools.AnyAsync(s => s.Domain == Normalize(domain) && s.PublicId != command.SchoolId, cancellationToken))
                    .WithMessage(_ => ValidationMessages.SchoolDomainTaken)
                    .When(c => !string.IsNullOrWhiteSpace(c.Domain)));

        // The platform is reached on the master tenant's domain, so it never changes or goes away.
        RuleFor(c => c.Domain)
            .MustAsync(async (command, domain, cancellationToken) =>
                !await context.Schools.AnyAsync(s => s.PublicId == command.SchoolId && s.IsMaster && s.Domain != Normalize(domain), cancellationToken))
            .WithMessage(_ => ValidationMessages.SchoolMasterDomainLocked);
    }

    /// <summary>The same normalisation the entity applies, so the checks compare like with like.</summary>
    private static string Normalize(string? domain) => (domain ?? string.Empty).Trim().TrimEnd('.').ToLowerInvariant();
}

public class UpdateSchoolCommandHandler : IRequestHandler<UpdateSchoolCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;

    public UpdateSchoolCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog)
    {
        _context = context;
        _auditLog = auditLog;
    }

    public async Task Handle(UpdateSchoolCommand request, CancellationToken cancellationToken)
    {
        var school = await _context.Schools.FirstOrDefaultAsync(s => s.PublicId == request.SchoolId, cancellationToken);
        Guard.Against.NotFound(request.SchoolId, school);

        school.UpdateProfile(request.Name, request.Domain);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.SchoolProfileChanged,
            SecurityAuditOutcome.Success,
            targetType: nameof(School),
            targetId: school.Id.ToString(CultureInfo.InvariantCulture));
    }
}
