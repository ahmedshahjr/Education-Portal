using EducationPortal.Application.Common.Security;
using EducationPortal.Application.Invoices.Commands.CreateInvoice;
using EducationPortal.Application.Invoices.Commands.RecordPayment;
using EducationPortal.Application.Invoices.Commands.VoidInvoice;
using EducationPortal.Application.Invoices.Common;
using EducationPortal.Application.Invoices.Queries.GetInvoice;
using EducationPortal.Application.Invoices.Queries.GetInvoices;
using EducationPortal.Web.Filters;
using Microsoft.AspNetCore.Mvc;

namespace EducationPortal.Web.Controllers;

/// <summary>
/// Invoices for school subscriptions and the manual payments recorded against them. Master tenant administrators only
/// (SEC-AUTHZ-07). Paying an invoice activates or extends the school's subscription.
/// </summary>
[Route("api/Invoices")]
[Microsoft.AspNetCore.Authorization.Authorize]
public class InvoicesController(
    IRequestRules rules,
    GetInvoicesQueryHandler getInvoices,
    GetInvoiceQueryHandler getInvoice,
    CreateInvoiceCommandHandler createInvoice,
    RecordPaymentCommandHandler recordPayment,
    VoidInvoiceCommandHandler voidInvoice) : ApiControllerBase
{
    [HttpGet]
    [EndpointSummary("List invoices")]
    [EndpointDescription("Returns invoices newest first, each with its lines and payments. Pass schoolId to list one school's. Master tenant only.")]
    public async Task<ActionResult<IReadOnlyList<InvoiceDto>>> GetInvoices(Guid? schoolId, CancellationToken cancellationToken)
    {
        var query = new GetInvoicesQuery(schoolId);
        await rules.CheckAsync(query, cancellationToken);
        return Ok(await getInvoices.Handle(query, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [EndpointSummary("Get an invoice")]
    [EndpointDescription("Returns one invoice with its lines and payments. Master tenant only.")]
    public async Task<ActionResult<InvoiceDto>> GetInvoice(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetInvoiceQuery(id);
        await rules.CheckAsync(query, cancellationToken);
        return Ok(await getInvoice.Handle(query, cancellationToken));
    }

    [HttpPost]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [EndpointSummary("Issue an invoice")]
    [EndpointDescription("Issues an invoice for the school's subscription and returns its id: for the current period, or the next one with nextPeriod true (the only choice while on trial). The amount is the subscription's price and the tax rate comes from the country's price book. One open invoice per subscription. Master tenant only.")]
    public async Task<ActionResult<Guid>> CreateInvoice(CreateInvoiceCommand command, CancellationToken cancellationToken)
    {
        await rules.CheckAsync(command, cancellationToken);
        return Ok(await createInvoice.Handle(command, cancellationToken));
    }

    [HttpPost("{id:guid}/payments")]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [EndpointSummary("Record a payment")]
    [EndpointDescription("Records a manual payment (bank transfer, cash, cheque, wallet) for an open invoice and returns the payment's id. The amount must equal the invoice total. The invoice becomes Paid and the school's subscription is activated or extended. Master tenant only.")]
    public async Task<ActionResult<Guid>> RecordPayment(Guid id, RecordPaymentCommand command, CancellationToken cancellationToken)
    {
        // The route names the invoice; an id in the body is ignored. The rules check the request as it will run.
        var request = command with { InvoiceId = id };
        await rules.CheckAsync(request, cancellationToken);
        return Ok(await recordPayment.Handle(request, cancellationToken));
    }

    [HttpPost("{id:guid}/void")]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [EndpointSummary("Void an invoice")]
    [EndpointDescription("Cancels an open invoice; the row is kept and the period can be invoiced again. A paid invoice cannot be voided. Master tenant only.")]
    public async Task<IActionResult> VoidInvoice(Guid id, CancellationToken cancellationToken)
    {
        var command = new VoidInvoiceCommand(id);
        await rules.CheckAsync(command, cancellationToken);
        await voidInvoice.Handle(command, cancellationToken);
        return NoContent();
    }
}
