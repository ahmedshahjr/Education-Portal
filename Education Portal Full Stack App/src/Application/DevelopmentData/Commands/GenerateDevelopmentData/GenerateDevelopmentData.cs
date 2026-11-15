using EducationPortal.Application.Common.Security;
using EducationPortal.Application.DevelopmentData.Common;
using EducationPortal.Domain.Constants;
using FluentValidation;
using MediatR;

namespace EducationPortal.Application.DevelopmentData.Commands.GenerateDevelopmentData;

/// <summary>
/// Creates many schools, campuses and people in one call, for development and load testing. The endpoint only exists in
/// Development. Master tenant administrators only (SEC-AUTHZ-07). Every limit below is checked before anything is written,
/// so one call cannot create more than <see cref="MaximumPeople"/> people.
/// </summary>
[Authorize(Roles = Roles.Administrator, MasterTenant = true)]
public record GenerateDevelopmentDataCommand : IRequest<DevelopmentDataSummary>
{
    public int Schools { get; init; }

    public int CampusesPerSchool { get; init; }

    public int PeoplePerCampus { get; init; }

    public string CountryCode { get; init; } = string.Empty;

    public int? Seed { get; init; }
}

public class GenerateDevelopmentDataCommandValidator : AbstractValidator<GenerateDevelopmentDataCommand>
{
    public const int MaximumSchools = 500;
    public const int MaximumCampusesPerSchool = 50;
    public const int MaximumPeoplePerCampus = 5000;
    public const int MaximumPeople = 1_000_000;
    public const int CountryCodeLength = 2;

    public GenerateDevelopmentDataCommandValidator()
    {
        RuleFor(c => c.Schools).InclusiveBetween(1, MaximumSchools);
        RuleFor(c => c.CampusesPerSchool).InclusiveBetween(1, MaximumCampusesPerSchool);
        RuleFor(c => c.PeoplePerCampus).InclusiveBetween(0, MaximumPeoplePerCampus);
        RuleFor(c => c.CountryCode).NotEmpty().Length(CountryCodeLength);

        // The product is checked on the whole request, so many small values cannot exceed the limit either.
        RuleFor(c => c)
            .Must(c => (long)c.Schools * c.CampusesPerSchool * c.PeoplePerCampus <= MaximumPeople)
            .WithMessage($"A request may create at most {MaximumPeople} people.");
    }
}

public class GenerateDevelopmentDataCommandHandler : IRequestHandler<GenerateDevelopmentDataCommand, DevelopmentDataSummary>
{
    private readonly IDevelopmentDataGenerator _generator;

    public GenerateDevelopmentDataCommandHandler(IDevelopmentDataGenerator generator)
    {
        _generator = generator;
    }

    public Task<DevelopmentDataSummary> Handle(GenerateDevelopmentDataCommand request, CancellationToken cancellationToken)
    {
        return _generator.GenerateAsync(
            new DevelopmentDataRequest(request.Schools, request.CampusesPerSchool, request.PeoplePerCampus, request.CountryCode.ToUpperInvariant(), request.Seed),
            cancellationToken);
    }
}
