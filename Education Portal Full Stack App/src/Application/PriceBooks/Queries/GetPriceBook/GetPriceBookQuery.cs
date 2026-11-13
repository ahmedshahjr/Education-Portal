using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Application.PriceBooks.Common;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.PriceBooks.Queries.GetPriceBook;

/// <summary>Returns one price book with its prices. Master tenant administrators only (SEC-AUTHZ-07).</summary>
[Authorize(Roles = Roles.Administrator, MasterTenant = true)]
public record GetPriceBookQuery(Guid Id) : IRequest<PriceBookDto>;

public class GetPriceBookQueryHandler : IRequestHandler<GetPriceBookQuery, PriceBookDto>
{
    private readonly IApplicationDbContext _context;

    public GetPriceBookQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PriceBookDto> Handle(GetPriceBookQuery request, CancellationToken cancellationToken)
    {
        var books = await PriceBookReader.LoadAsync(_context, request.Id, cancellationToken);
        var book = books.FirstOrDefault();
        Guard.Against.NotFound(request.Id, book);

        return book;
    }
}
