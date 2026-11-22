using System.Globalization;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;
using FluentValidation.Results;

namespace EducationPortal.Application.Schools.Commands.UpdateSchoolDetails;

/// <summary>
/// Replaces a school's registration and contact details. Its administrator changes their own school; the master tenant names
/// the school. The city must be in the school's country. Needs the School module. Audited (SEC-LOG-03).
/// </summary>
[Authorize(Roles = Roles.Administrator, Module = ModuleCode.School)]
public record UpdateSchoolDetailsCommand
{
    /// <summary>The school's public id. The master tenant must send it; a school user may leave it out.</summary>
    public Guid? SchoolId { get; init; }

    public string? LocalName { get; init; }

    public string? RegistrationNumber { get; init; }

    public string? RegistrationAuthority { get; init; }

    public string? TaxNumber { get; init; }

    public string? Address { get; init; }

    /// <summary>The city's public id from the location data, or <c>null</c>.</summary>
    public Guid? CityId { get; init; }

    public string? Phone { get; init; }

    public string? Email { get; init; }

    public string? Website { get; init; }

    public int AcademicYearStartMonth { get; init; } = SchoolDetails.DefaultAcademicYearStartMonth;
}

public class UpdateSchoolDetailsCommandValidator : AbstractValidator<UpdateSchoolDetailsCommand>
{
    public UpdateSchoolDetailsCommandValidator()
    {
        RuleFor(c => c.LocalName).MaximumLength(SchoolDetails.LocalNameMaxLength);
        RuleFor(c => c.RegistrationNumber).MaximumLength(SchoolDetails.RegistrationNumberMaxLength);
        RuleFor(c => c.RegistrationAuthority).MaximumLength(SchoolDetails.RegistrationAuthorityMaxLength);
        RuleFor(c => c.TaxNumber).MaximumLength(SchoolDetails.TaxNumberMaxLength);
        RuleFor(c => c.Address).MaximumLength(SchoolDetails.AddressMaxLength);
        RuleFor(c => c.CityId).NotEqual(Guid.Empty).When(c => c.CityId.HasValue);

        RuleFor(c => c.Phone)
            .Must(value => string.IsNullOrWhiteSpace(value) || ContactRules.IsValidPhone(value.Trim()))
            .WithMessage(_ => ValidationMessages.PhoneInvalid);

        RuleFor(c => c.Email)
            .Must(value => string.IsNullOrWhiteSpace(value) || ContactRules.IsValidEmail(value.Trim()))
            .WithMessage(_ => ValidationMessages.EmailInvalid);

        RuleFor(c => c.Website)
            .Must(value => string.IsNullOrWhiteSpace(value) || ContactRules.IsValidWebsite(value.Trim()))
            .WithMessage(_ => ValidationMessages.WebsiteInvalid);

        RuleFor(c => c.AcademicYearStartMonth).InclusiveBetween(1, 12);
    }
}

public class UpdateSchoolDetailsCommandHandler
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;
    private readonly ICurrentTenant _tenant;

    public UpdateSchoolDetailsCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog, ICurrentTenant tenant)
    {
        _context = context;
        _auditLog = auditLog;
        _tenant = tenant;
    }

    public async Task Handle(UpdateSchoolDetailsCommand request, CancellationToken cancellationToken)
    {
        var schoolId = await SchoolScope.ResolveAsync(_context, _tenant, request.SchoolId, cancellationToken);
        var school = await _context.Schools.FirstAsync(s => s.Id == schoolId, cancellationToken);

        int? cityId = null;
        if (request.CityId is { } cityPublicId)
        {
            // The city must be in a state of the school's country; any other city is a validation error.
            cityId = await _context.Cities
                .Where(c => c.PublicId == cityPublicId)
                .Join(_context.States, c => c.StateId, st => st.Id, (c, st) => new { c.Id, st.CountryId })
                .Where(x => school.CountryId != null && x.CountryId == school.CountryId)
                .Select(x => (int?)x.Id)
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new EducationPortal.Application.Common.Exceptions.ValidationException(
                    [new ValidationFailure(nameof(request.CityId), ValidationMessages.CityNotInSchoolCountry)]);
        }

        school.UpdateDetails(SchoolDetails.Create(
            request.LocalName,
            request.RegistrationNumber,
            request.RegistrationAuthority,
            request.TaxNumber,
            request.Address,
            cityId,
            request.Phone,
            request.Email,
            request.Website,
            request.AcademicYearStartMonth));
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.SchoolDetailsChanged,
            SecurityAuditOutcome.Success,
            targetType: nameof(School),
            targetId: school.Id.ToString(CultureInfo.InvariantCulture));
    }
}
