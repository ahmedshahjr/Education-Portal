using EducationPortal.Application.Academics.Common;
using EducationPortal.Application.Common.Exceptions;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Localization;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Academics;
using EducationPortal.Domain.Enums;
using FluentValidation.Results;

namespace EducationPortal.Application.Academics.Commands.CreateGradingScale;

/// <summary>Creates a grading scale of the school. Its bands must cover 0 to 100 once, with no gap and no overlap. Needs the Exams module and the Create right. Audited.</summary>
[Authorize(Module = ModuleCode.Exams, Right = AccessRight.Create)]
public record CreateGradingScaleCommand
{
    public string Name { get; init; } = string.Empty;

    public IReadOnlyList<GradeBandDto> Bands { get; init; } = [];
}

public class CreateGradingScaleCommandValidator : AbstractValidator<CreateGradingScaleCommand>
{
    public CreateGradingScaleCommandValidator()
    {
        RuleFor(c => c.Name).NotEmpty().MaximumLength(Domain.Entities.GradingScale.NameMaxLength);
        RuleFor(c => c.Bands).NotEmpty();
        RuleFor(c => c).Custom((command, context) =>
        {
            if (command.Bands.Count == 0)
            {
                return;
            }

            try
            {
                GradeScale.Validate(command.Bands.Select(b => new GradeBand(b.MinPercent, b.MaxPercent, b.Letter, b.Points)).ToList());
            }
            catch (ArgumentException)
            {
                // The rule's own message is translated; the exception text is never shown to the user.
                context.AddFailure(new ValidationFailure(nameof(command.Bands), ValidationMessages.GradeScaleInvalid));
            }
        });
    }
}

public class CreateGradingScaleCommandHandler
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenant _tenant;

    public CreateGradingScaleCommandHandler(IApplicationDbContext context, ICurrentTenant tenant)
    {
        _context = context;
        _tenant = tenant;
    }

    public async Task<Guid> Handle(CreateGradingScaleCommand request, CancellationToken cancellationToken)
    {
        var schoolId = _tenant.SchoolId ?? throw new ForbiddenAccessException();

        var bands = request.Bands.Select(b => new GradeBand(b.MinPercent, b.MaxPercent, b.Letter, b.Points)).ToList();

        var scale = Domain.Entities.GradingScale.Create(schoolId, request.Name, bands);
        _context.GradingScales.Add(scale);
        await _context.SaveChangesAsync(cancellationToken);

        return scale.PublicId;
    }
}
