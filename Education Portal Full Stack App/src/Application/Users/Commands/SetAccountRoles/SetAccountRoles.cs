using EducationPortal.Application.Common.Exceptions;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Localization;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Enums;
using FluentValidation.Results;

namespace EducationPortal.Application.Users.Commands.SetAccountRoles;

/// <summary>
/// Sets the school roles a person's login account holds: exactly the given roles, so the account gets the roles it is given
/// and loses the others. The person must already have an account (created with the person). Needs the Users &amp; Roles
/// module. The change is recorded by the change audit.
/// </summary>
[Authorize(Roles = Roles.Administrator, Module = ModuleCode.UsersRoles)]
public record SetAccountRolesCommand : IRequest
{
    public Guid PersonId { get; init; }

    public IReadOnlyList<Guid> RoleIds { get; init; } = [];
}

public class SetAccountRolesCommandValidator : AbstractValidator<SetAccountRolesCommand>
{
    public const int MaxRoles = 50;

    public SetAccountRolesCommandValidator()
    {
        RuleFor(c => c.PersonId).NotEmpty();
        RuleFor(c => c.RoleIds)
            .NotNull()
            .Must(ids => ids.Count <= MaxRoles)
            .Must(ids => ids.Distinct().Count() == ids.Count)
            .WithMessage(_ => ValidationMessages.ModuleListedTwice);
    }
}

public class SetAccountRolesCommandHandler : IRequestHandler<SetAccountRolesCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;
    private readonly ICurrentTenant _tenant;

    public SetAccountRolesCommandHandler(IApplicationDbContext context, IIdentityService identityService, ICurrentTenant tenant)
    {
        _context = context;
        _identityService = identityService;
        _tenant = tenant;
    }

    public async Task Handle(SetAccountRolesCommand request, CancellationToken cancellationToken)
    {
        var schoolId = _tenant.SchoolId ?? throw new ForbiddenAccessException();

        var personId = await _context.UserDetails.ResolveIdAsync(request.PersonId, cancellationToken);
        var accountId = await _identityService.FindAccountIdOfPersonAsync(schoolId, personId);
        if (accountId is null)
        {
            throw new EducationPortal.Application.Common.Exceptions.ValidationException(
                [new ValidationFailure(nameof(request.PersonId), ValidationMessages.PersonHasNoAccount)]);
        }

        var roleIds = await _context.SchoolRoles
            .Where(r => r.SchoolId == schoolId && request.RoleIds.Contains(r.PublicId))
            .Select(r => r.Id)
            .ToListAsync(cancellationToken);
        if (roleIds.Count != request.RoleIds.Count)
        {
            throw new EducationPortal.Application.Common.Exceptions.ValidationException(
                [new ValidationFailure(nameof(request.RoleIds), ValidationMessages.RoleNotInSchool)]);
        }

        var current = await _context.AccountRoles.Where(a => a.UserId == accountId).ToListAsync(cancellationToken);

        foreach (var grant in current.Where(g => !roleIds.Contains(g.SchoolRoleId)))
        {
            _context.AccountRoles.Remove(grant);
        }

        foreach (var roleId in roleIds.Where(id => current.All(g => g.SchoolRoleId != id)))
        {
            _context.AccountRoles.Add(AccountRole.Create(schoolId, accountId, roleId));
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
