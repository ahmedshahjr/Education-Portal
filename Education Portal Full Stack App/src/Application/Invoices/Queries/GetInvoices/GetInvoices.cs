using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Application.Invoices.Common;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.Invoices.Queries.GetInvoices;

/// <summary>
/// Lists invoices newest first: every school's, or one school's when a school id is given. Master tenant administrators
/// only (SEC-AUTHZ-07), so the school id may come from the query string (SEC-AUTHZ-04).
/// </summary>
[Authorize(Roles = Roles.Administrator, MasterTenant = true)]
public record GetInvoicesQuery(Guid? SchoolId) : IRequest<IReadOnlyList<InvoiceDto>>;

public class GetInvoicesQueryHandler : IRequestHandler<GetInvoicesQuery, IReadOnlyList<InvoiceDto>>
{
    private readonly IApplicationDbContext _context;

    public GetInvoicesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<InvoiceDto>> Handle(GetInvoicesQuery request, CancellationToken cancellationToken) =>
        await InvoiceReader.LoadAsync(_context, request.SchoolId, null, cancellationToken);
}
