using System.Globalization;
using EducationPortal.Application.Common.Exceptions;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Localization;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using EducationPortal.Domain.ValueObjects;
using FluentValidation.Results;

namespace EducationPortal.Application.Users.Commands.CreateUserDetails;

/// <summary>
/// Adds a person (a principal, a teacher or other staff) to the current school. The school is the one the request's host
/// resolves to (SEC-AUTHZ-04). Needs the Staff module. Audited (SEC-LOG-03) with the person's id only, not the name.
/// </summary>
[Authorize(Roles = Roles.Administrator, Module = ModuleCodes.Staff)]
public record CreateUserDetailsCommand : IRequest<Guid>
{
    public string FirstName { get; init; } = string.Empty;

    /// <summary>Optional.</summary>
    public string? MiddleName { get; init; }

    public string LastName { get; init; } = string.Empty;

    public UserType UserType { get; init; }

    public string? Phone { get; init; }
}

public class CreateUserDetailsCommandValidator : AbstractValidator<CreateUserDetailsCommand>
{
    public CreateUserDetailsCommandValidator()
    {
        RuleFor(c => c.FirstName)
            .NotEmpty()
            .MaximumLength(UserDetails.NameMaxLength);

        RuleFor(c => c.MiddleName)
            .MaximumLength(UserDetails.NameMaxLength);

        RuleFor(c => c.LastName)
            .NotEmpty()
            .MaximumLength(UserDetails.NameMaxLength);

        RuleFor(c => c.UserType).IsInEnum();

        RuleFor(c => c.Phone)
            .MaximumLength(ContactRules.PhoneMaxLength)
            .Must(value => string.IsNullOrWhiteSpace(value) || ContactRules.IsValidPhone(value.Trim()))
            .WithMessage(_ => ValidationMessages.PhoneInvalid);
    }
}

public class CreateUserDetailsCommandHandler : IRequestHandler<CreateUserDetailsCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;
    private readonly ICurrentTenant _tenant;

    public CreateUserDetailsCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog, ICurrentTenant tenant)
    {
        _context = context;
        _auditLog = auditLog;
        _tenant = tenant;
    }

    public async Task<Guid> Handle(CreateUserDetailsCommand request, CancellationToken cancellationToken)
    {
        var schoolId = _tenant.SchoolId ?? throw new ForbiddenAccessException();

        var person = UserDetails.Create(schoolId, request.FirstName, request.MiddleName, request.LastName, request.UserType, request.Phone);
        _context.UserDetails.Add(person);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.UserDetailsCreated,
            SecurityAuditOutcome.Success,
            targetType: nameof(UserDetails),
            targetId: person.Id.ToString(CultureInfo.InvariantCulture));

        return person.PublicId;
    }
}
