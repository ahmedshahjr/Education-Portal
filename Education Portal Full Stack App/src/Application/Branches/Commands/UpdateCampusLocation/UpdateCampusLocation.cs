using System.Globalization;
using EducationPortal.Application.Common.Exceptions;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;
using FluentValidation.Results;

namespace EducationPortal.Application.Branches.Commands.UpdateCampusLocation;

/// <summary>
/// Sets where a campus of the current school is: its province and city (from the location data), an address line, and the
/// pin on the map (latitude and longitude, with the Google place id and the Google Maps link). Every part is optional. Needs
/// the Branches module. Audited (SEC-LOG-03).
/// </summary>
[Authorize(Roles = Roles.Administrator, Module = ModuleCodes.Branch)]
public record UpdateCampusLocationCommand : IRequest
{
    /// <summary>The campus's public id, taken from the route (ENG-ID-02).</summary>
    public Guid BranchId { get; init; }

    /// <summary>The province, from the location data. Required when a city is given.</summary>
    public Guid? StateId { get; init; }

    /// <summary>The city, from the location data. It must be in <see cref="StateId"/>.</summary>
    public Guid? CityId { get; init; }

    public string? AddressLine { get; init; }

    /// <summary>Latitude in decimal degrees (-90 to 90). Given together with <see cref="Longitude"/>.</summary>
    public decimal? Latitude { get; init; }

    /// <summary>Longitude in decimal degrees (-180 to 180). Given together with <see cref="Latitude"/>.</summary>
    public decimal? Longitude { get; init; }

    public string? GooglePlaceId { get; init; }

    /// <summary>A https link to the pinned location on Google Maps.</summary>
    public string? GoogleMapsUrl { get; init; }
}

public class UpdateCampusLocationCommandValidator : AbstractValidator<UpdateCampusLocationCommand>
{
    public UpdateCampusLocationCommandValidator()
    {
        RuleFor(c => c.BranchId).NotEmpty();

        RuleFor(c => c.AddressLine)
            .MaximumLength(Branch.AddressMaxLength)
            .WithMessage(_ => EducationPortal.Application.Common.Localization.ValidationMessages.CampusAddressTooLong);

        RuleFor(c => c.Latitude)
            .InclusiveBetween(-90m, 90m)
            .When(c => c.Latitude is not null);

        RuleFor(c => c.Longitude)
            .InclusiveBetween(-180m, 180m)
            .When(c => c.Longitude is not null);

        RuleFor(c => c)
            .Must(c => (c.Latitude is null) == (c.Longitude is null))
            .WithMessage(_ => EducationPortal.Application.Common.Localization.ValidationMessages.CampusCoordinatesTogether);

        RuleFor(c => c)
            .Must(c => c.CityId is null || c.StateId is not null)
            .WithMessage(_ => EducationPortal.Application.Common.Localization.ValidationMessages.CampusCityNeedsState);

        RuleFor(c => c.GooglePlaceId)
            .MaximumLength(Branch.GooglePlaceIdMaxLength);

        RuleFor(c => c.GoogleMapsUrl)
            .MaximumLength(Branch.GoogleMapsUrlMaxLength)
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps)
            .When(c => !string.IsNullOrWhiteSpace(c.GoogleMapsUrl))
            .WithMessage(_ => EducationPortal.Application.Common.Localization.ValidationMessages.CampusGoogleMapsUrlFormat);
    }
}

public class UpdateCampusLocationCommandHandler : IRequestHandler<UpdateCampusLocationCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;
    private readonly ICurrentTenant _tenant;

    public UpdateCampusLocationCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog, ICurrentTenant tenant)
    {
        _context = context;
        _auditLog = auditLog;
        _tenant = tenant;
    }

    public async Task Handle(UpdateCampusLocationCommand request, CancellationToken cancellationToken)
    {
        var schoolId = _tenant.SchoolId ?? throw new ForbiddenAccessException();
        var branch = await _context.Branches.FirstOrDefaultAsync(b => b.SchoolId == schoolId && b.PublicId == request.BranchId, cancellationToken);
        Guard.Against.NotFound(request.BranchId, branch);

        int? stateId = null;
        int? cityId = null;

        if (request.StateId is { } stateGuid)
        {
            var state = await _context.States.AsNoTracking().FirstOrDefaultAsync(s => s.PublicId == stateGuid, cancellationToken);
            Guard.Against.NotFound(stateGuid, state);
            stateId = state.Id;

            if (request.CityId is { } cityGuid)
            {
                var city = await _context.Cities.AsNoTracking().FirstOrDefaultAsync(c => c.PublicId == cityGuid, cancellationToken);
                Guard.Against.NotFound(cityGuid, city);

                // A city must belong to the province the campus is in (the location data is one tree, not a free list).
                if (city.StateId != state.Id)
                {
                    throw new EducationPortal.Application.Common.Exceptions.ValidationException([new ValidationFailure(nameof(request.CityId), EducationPortal.Application.Common.Localization.ValidationMessages.CampusCityNotInState)]);
                }

                cityId = city.Id;
            }
        }
        else if (request.CityId is not null)
        {
            throw new EducationPortal.Application.Common.Exceptions.ValidationException([new ValidationFailure(nameof(request.CityId), EducationPortal.Application.Common.Localization.ValidationMessages.CampusCityNeedsState)]);
        }

        branch.SetLocation(
            stateId,
            cityId,
            request.AddressLine,
            request.Latitude,
            request.Longitude,
            request.GooglePlaceId,
            request.GoogleMapsUrl);

        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.BranchUpdated,
            SecurityAuditOutcome.Success,
            targetType: nameof(Branch),
            targetId: branch.Id.ToString(CultureInfo.InvariantCulture));
    }
}
