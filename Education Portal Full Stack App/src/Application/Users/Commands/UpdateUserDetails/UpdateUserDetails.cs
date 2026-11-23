using System.Globalization;
using EducationPortal.Application.Common.Exceptions;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Application.Users.Common;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.Users.Commands.UpdateUserDetails;

/// <summary>
/// Changes a person of the current school. The person is named by <see cref="Id"/> (the public id from the route). The school
/// is the one the request's host resolves to, so another school's person is not found (SEC-AUTHZ-03). Needs the Staff module.
/// Audited (SEC-LOG-03) with the person's id only, not the name.
/// </summary>
[Authorize(Roles = Roles.Administrator, Module = ModuleCode.Staff)]
public record UpdateUserDetailsCommand : UserDetailsFields
{
    /// <summary>The person's public id.</summary>
    public Guid Id { get; init; }
}

public class UpdateUserDetailsCommandValidator : UserDetailsFieldsValidator<UpdateUserDetailsCommand>
{
    public UpdateUserDetailsCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
    }
}

public class UpdateUserDetailsCommandHandler
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;
    private readonly ICurrentTenant _tenant;

    public UpdateUserDetailsCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog, ICurrentTenant tenant)
    {
        _context = context;
        _auditLog = auditLog;
        _tenant = tenant;
    }

    public async Task Handle(UpdateUserDetailsCommand request, CancellationToken cancellationToken)
    {
        _ = _tenant.SchoolId;

        // The tenant filter hides another school's people, so they are not found here.
        var person = await _context.UserDetails.FirstOrDefaultAsync(u => u.PublicId == request.Id, cancellationToken);
        Guard.Against.NotFound(request.Id, person);

        person.Update(request.FirstName, request.MiddleName, request.LastName, request.UserType, request.Phone);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.UserDetailsUpdated,
            SecurityAuditOutcome.Success,
            targetType: nameof(UserDetails),
            targetId: person.Id.ToString(CultureInfo.InvariantCulture));
    }
}
