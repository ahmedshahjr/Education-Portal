using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Application.Invoices.Common;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.Invoices.Queries.GetInvoice;

/// <summary>Returns one invoice with its lines and payments; 404 when it does not exist. Master tenant administrators only (SEC-AUTHZ-07).</summary>
[Authorize(Roles = Roles.Administrator, MasterTenant = true)]
public record GetInvoiceQuery(Guid Id) : IRequest<InvoiceDto>;

public class GetInvoiceQueryHandler : IRequestHandler<GetInvoiceQuery, InvoiceDto>
{
    private readonly IApplicationDbContext _context;

    public GetInvoiceQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<InvoiceDto> Handle(GetInvoiceQuery request, CancellationToken cancellationToken)
    {
        var invoice = (await InvoiceReader.LoadAsync(_context, null, request.Id, cancellationToken)).FirstOrDefault();
        Guard.Against.NotFound(request.Id, invoice);
        return invoice;
    }
}
