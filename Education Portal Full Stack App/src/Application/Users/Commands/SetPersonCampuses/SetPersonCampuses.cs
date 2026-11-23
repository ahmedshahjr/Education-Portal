using EducationPortal.Application.Common.Exceptions;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Localization;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Enums;
using FluentValidation.Results;

namespace EducationPortal.Application.Users.Commands.SetPersonCampuses;

/// <summary>
/// Sets the campuses a person of the school is assigned to: exactly the given ones, so the person gets the campuses they
/// are given and loses the others. A person who is not an administrator acts only on these campuses (AGENTS.md section 2,
/// "Campuses per user"). Needs the Users &amp; Roles module. The change is recorded by the change audit.
/// </summary>
[Authorize(Roles = Roles.Administrator, Module = ModuleCode.UsersRoles)]
public record SetPersonCampusesCommand
{
    public Guid PersonId { get; init; }

    public IReadOnlyList<Guid> CampusIds { get; init; } = [];
}

public class SetPersonCampusesCommandValidator : AbstractValidator<SetPersonCampusesCommand>
{
    /// <summary>The longest list one request can set.</summary>
    public const int MaxCampuses = 200;

    public SetPersonCampusesCommandValidator()
    {
        RuleFor(c => c.PersonId).NotEmpty();
        RuleFor(c => c.CampusIds)
            .NotNull()
            .Must(ids => ids.Count <= MaxCampuses)
            .Must(ids => ids.Distinct().Count() == ids.Count)
            .WithMessage(_ => ValidationMessages.CampusListedTwice);
    }
}

public class SetPersonCampusesCommandHandler
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenant _tenant;

    public SetPersonCampusesCommandHandler(IApplicationDbContext context, ICurrentTenant tenant)
    {
        _context = context;
        _tenant = tenant;
    }

    public async Task Handle(SetPersonCampusesCommand request, CancellationToken cancellationToken)
    {
        var schoolId = _tenant.SchoolId;

        // The tenant filter hides another school's person, so their id is not found (SEC-AUTHZ-03).
        var personId = await _context.UserDetails.ResolveIdAsync(request.PersonId, cancellationToken);

        var campusIds = await _context.Campuses
            .Where(c => c.SchoolId == schoolId && request.CampusIds.Contains(c.PublicId))
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);
        if (campusIds.Count != request.CampusIds.Count)
        {
            throw new EducationPortal.Application.Common.Exceptions.ValidationException(
                [new ValidationFailure(nameof(request.CampusIds), ValidationMessages.CampusNotInSchool)]);
        }

        var current = await _context.UserCampuses.Where(u => u.UserDetailsId == personId).ToListAsync(cancellationToken);

        foreach (var link in current.Where(l => !campusIds.Contains(l.CampusId)))
        {
            _context.UserCampuses.Remove(link);
        }

        foreach (var campusId in campusIds.Where(id => current.All(l => l.CampusId != id)))
        {
            _context.UserCampuses.Add(UserCampus.Create(schoolId, personId, campusId));
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
