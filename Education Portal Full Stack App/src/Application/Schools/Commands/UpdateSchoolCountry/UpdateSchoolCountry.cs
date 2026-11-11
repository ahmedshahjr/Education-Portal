using System.Globalization;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.Schools.Commands.UpdateSchoolCountry;

/// <summary>
/// Assigns a school to a country, which decides the languages, time zone and currency it uses, or to none (English
/// only, UTC). A master-tenant use case, so the school id may come from the route (SEC-AUTHZ-04, SEC-AUTHZ-07).
/// Audited (SEC-LOG-03).
/// </summary>
[Authorize(Roles = Roles.Administrator, MasterTenant = true)]
public record UpdateSchoolCountryCommand : IRequest
{
    /// <summary>The school's public id (ENG-ID-02).</summary>
    public Guid SchoolId { get; init; }

    /// <summary>The country, or <c>null</c> for the platform defaults.</summary>
    public Guid? CountryId { get; init; }
}

public class UpdateSchoolCountryCommandValidator : AbstractValidator<UpdateSchoolCountryCommand>
{
    public UpdateSchoolCountryCommandValidator(IApplicationDbContext context)
    {
        RuleFor(c => c.SchoolId).NotEmpty();

        RuleFor(c => c.CountryId)
            .MustAsync(async (countryId, cancellationToken) => await context.Countries.AnyAsync(c => c.PublicId == countryId, cancellationToken))
            .When(c => c.CountryId is not null)
            .WithMessage("The country does not exist.");
    }
}

public class UpdateSchoolCountryCommandHandler : IRequestHandler<UpdateSchoolCountryCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;

    public UpdateSchoolCountryCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog)
    {
        _context = context;
        _auditLog = auditLog;
    }

    public async Task Handle(UpdateSchoolCountryCommand request, CancellationToken cancellationToken)
    {
        var school = await _context.Schools.FirstOrDefaultAsync(s => s.PublicId == request.SchoolId, cancellationToken);
        Guard.Against.NotFound(request.SchoolId, school);

        long? countryId = request.CountryId is { } countryPublicId
            ? await _context.Countries.ResolveIdAsync(countryPublicId, cancellationToken)
            : null;

        school.AssignCountry(countryId);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.SchoolCountryChanged,
            SecurityAuditOutcome.Success,
            targetType: nameof(School),
            targetId: school.Id.ToString(CultureInfo.InvariantCulture));
    }
}
