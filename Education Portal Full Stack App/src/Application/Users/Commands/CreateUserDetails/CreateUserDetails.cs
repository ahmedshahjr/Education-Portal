using System.Globalization;
using EducationPortal.Application.Common.Exceptions;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Localization;
using EducationPortal.Application.Common.Security;
using EducationPortal.Application.Users.Common;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using FluentValidation.Results;

namespace EducationPortal.Application.Users.Commands.CreateUserDetails;

/// <summary>
/// Adds a person (a principal, a teacher, other staff or a student) to the current school, and, when an email and a
/// one-time password are given, their login account, linked to the person. The person and the account are saved in one
/// transaction: a weak password refuses both. The school is the one the request's host resolves to (SEC-AUTHZ-04). Needs
/// the Staff module. Audited (SEC-LOG-03) with ids only, not the name or the email.
/// </summary>
[Authorize(Roles = Roles.Administrator, Module = ModuleCode.Staff)]
public record CreateUserDetailsCommand : UserDetailsFields
{
    /// <summary>The person's login email. Optional; when given, an account is created with <see cref="OneTimePassword"/>.</summary>
    public string? Email { get; init; }

    /// <summary>A one-time password the person replaces on first sign-in. Required with <see cref="Email"/>.</summary>
    public string? OneTimePassword { get; init; }
}

public class CreateUserDetailsCommandValidator : UserDetailsFieldsValidator<CreateUserDetailsCommand>
{
    public const int EmailMaxLength = 256;

    public CreateUserDetailsCommandValidator(IIdentityService identityService)
    {
        // A password without an email is refused here: an account needs its email.
        RuleFor(c => c.Email)
            .NotEmpty()
            .When(c => !string.IsNullOrEmpty(c.OneTimePassword));

        RuleFor(c => c.Email)
            .MaximumLength(EmailMaxLength)
            .EmailAddress()
            .When(c => !string.IsNullOrWhiteSpace(c.Email));

        RuleFor(c => c.Email)
            .MustAsync(async (email, _) => !await identityService.IsEmailTakenAsync(email!.Trim()))
            .WithMessage(_ => ValidationMessages.EmailTaken)
            .When(c => !string.IsNullOrWhiteSpace(c.Email));

        // Identity checks the full password policy when the account is made; these are the length limits.
        RuleFor(c => c.OneTimePassword)
            .NotEmpty()
            .MinimumLength(PasswordRules.MinimumLength)
            .MaximumLength(PasswordRules.MaximumLength)
            .When(c => !string.IsNullOrWhiteSpace(c.Email));
    }
}

public class CreateUserDetailsCommandHandler
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;
    private readonly ISecurityAuditLog _auditLog;
    private readonly ICurrentTenant _tenant;

    public CreateUserDetailsCommandHandler(IApplicationDbContext context, IIdentityService identityService, ISecurityAuditLog auditLog, ICurrentTenant tenant)
    {
        _context = context;
        _identityService = identityService;
        _auditLog = auditLog;
        _tenant = tenant;
    }

    public async Task<Guid> Handle(CreateUserDetailsCommand request, CancellationToken cancellationToken)
    {
        var schoolId = _tenant.SchoolId ?? throw new ForbiddenAccessException();

        var person = UserDetails.Create(schoolId, request.FirstName, request.MiddleName, request.LastName, request.UserType, request.Phone);

        // One transaction: the person and their account are saved together or not at all.
        var accountId = await _context.ExecuteInTransactionAsync(async token =>
        {
            _context.UserDetails.Add(person);
            await _context.SaveChangesAsync(token);

            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return null;
            }

            var (result, userId) = await _identityService.CreatePersonAccountAsync(
                schoolId, person.Id, request.Email.Trim(), request.OneTimePassword ?? string.Empty);

            if (!result.Succeeded)
            {
                // Throwing rolls the person back too. The policy messages say what to change in the password.
                throw new EducationPortal.Application.Common.Exceptions.ValidationException(result.Errors.Select(e => new ValidationFailure(nameof(request.OneTimePassword), e)));
            }

            return userId;
        }, cancellationToken);

        // Written after the commit, so the audit never describes a person that was rolled back.
        await _auditLog.WriteAsync(
            SecurityAuditEventType.UserDetailsCreated,
            SecurityAuditOutcome.Success,
            targetType: nameof(UserDetails),
            targetId: person.Id.ToString(CultureInfo.InvariantCulture));

        if (accountId is not null)
        {
            await _auditLog.WriteAsync(
                SecurityAuditEventType.UserCreated,
                SecurityAuditOutcome.Success,
                targetType: "User",
                targetId: accountId);
        }

        return person.PublicId;
    }
}
