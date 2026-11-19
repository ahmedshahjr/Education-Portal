using EducationPortal.Application.Academics.Common;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.Academics.Queries.GetGradingScales;

/// <summary>The school's grading scales, by name, each with its bands in order. Needs the Exams module and the View right.</summary>
[Authorize(Module = ModuleCode.Exams, Right = AccessRight.View)]
public record GetGradingScalesQuery : IRequest<IReadOnlyList<GradingScaleDto>>;

public class GetGradingScalesQueryHandler : IRequestHandler<GetGradingScalesQuery, IReadOnlyList<GradingScaleDto>>
{
    private readonly IApplicationDbContext _context;

    public GetGradingScalesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<GradingScaleDto>> Handle(GetGradingScalesQuery request, CancellationToken cancellationToken)
    {
        var scales = await _context.GradingScales.AsNoTracking()
            .OrderBy(s => s.Name)
            .Select(s => new
            {
                s.PublicId,
                s.Name,
                Bands = s.Bands.Select(b => new GradeBandDto(b.MinPercent, b.MaxPercent, b.Letter, b.Points)).ToList(),
            })
            .ToListAsync(cancellationToken);

        return scales
            .Select(s => new GradingScaleDto(
                s.PublicId,
                s.Name,
                s.Bands.OrderBy(b => b.MinPercent).ToList()))
            .ToList();
    }
}
