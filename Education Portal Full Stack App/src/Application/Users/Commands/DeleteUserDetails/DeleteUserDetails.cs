using System.Globalization;
using EducationPortal.Application.Common.Exceptions;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Localization;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;
using FluentValidation.Results;

namespace EducationPortal.Application.Users.Commands.DeleteUserDetails;

/// <summary>
/// Deletes a person of the current school (kept for audit, hidden everywhere). A person who is the principal of a campus
/// cannot be deleted until another principal is chosen there. The school is the one the request's host resolves to. Needs
/// the Staff module. Audited (SEC-LOG-03) with the person's id only.
/// </summary>
[Authorize(Roles = Roles.Administrator, Module = ModuleCode.Staff)]
public record DeleteUserDetailsCommand
{
    /// <summary>The person's public id.</summary>
    public Guid Id { get; init; }
}

public class DeleteUserDetailsCommandValidator : AbstractValidator<DeleteUserDetailsCommand>
{
    public DeleteUserDetailsCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
    }
}

public class DeleteUserDetailsCommandHandler
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;
    private readonly TimeProvider _timeProvider;
    private readonly ICurrentTenant _tenant;
    private readonly IUser _user;

    public DeleteUserDetailsCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog, TimeProvider timeProvider, ICurrentTenant tenant, IUser user)
    {
        _context = context;
        _auditLog = auditLog;
        _timeProvider = timeProvider;
        _tenant = tenant;
        _user = user;
    }

    public async Task Handle(DeleteUserDetailsCommand request, CancellationToken cancellationToken)
    {
        _ = _tenant.SchoolId ?? throw new ForbiddenAccessException();

        var person = await _context.UserDetails.FirstOrDefaultAsync(u => u.PublicId == request.Id, cancellationToken);
        Guard.Against.NotFound(request.Id, person);

        var isPrincipalOfACampus = await _context.Campuses.AnyAsync(c => c.PrincipalUserDetailsId == person.Id, cancellationToken);
        if (isPrincipalOfACampus)
        {
            throw new EducationPortal.Application.Common.Exceptions.ValidationException([new ValidationFailure(nameof(DeleteUserDetailsCommand.Id), ValidationMessages.UserDetailsIsPrincipal)]);
        }

        person.Delete(_timeProvider.GetUtcNow(), _user.Id);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.UserDetailsDeleted,
            SecurityAuditOutcome.Success,
            targetType: nameof(UserDetails),
            targetId: person.Id.ToString(CultureInfo.InvariantCulture));
    }
}
