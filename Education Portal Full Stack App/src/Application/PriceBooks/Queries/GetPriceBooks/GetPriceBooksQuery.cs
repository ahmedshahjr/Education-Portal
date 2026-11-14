using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Application.PriceBooks.Common;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.PriceBooks.Queries.GetPriceBooks;

/// <summary>Lists the price books by country, with their prices. Master tenant administrators only (SEC-AUTHZ-07).</summary>
[Authorize(Roles = Roles.Administrator, MasterTenant = true)]
public record GetPriceBooksQuery : IRequest<IReadOnlyList<PriceBookDto>>;

public class GetPriceBooksQueryHandler : IRequestHandler<GetPriceBooksQuery, IReadOnlyList<PriceBookDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPriceBooksQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<PriceBookDto>> Handle(GetPriceBooksQuery request, CancellationToken cancellationToken) =>
        await PriceBookReader.LoadAsync(_context, id: null, cancellationToken);
}
