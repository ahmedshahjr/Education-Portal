using EducationPortal.Application.Invoices.Commands.CreateInvoice;
using EducationPortal.Application.Invoices.Commands.RecordPayment;
using EducationPortal.Application.Invoices.Commands.VoidInvoice;
using EducationPortal.Application.Invoices.Common;
using EducationPortal.Application.Invoices.Queries.GetInvoice;
using EducationPortal.Application.Invoices.Queries.GetInvoices;
using MediatR;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http.HttpResults;

namespace EducationPortal.Web.Endpoints;

/// <summary>
/// Invoices for school subscriptions and the manual payments recorded against them. Master tenant administrators only
/// (SEC-AUTHZ-07). Paying an invoice activates or extends the school's subscription.
/// </summary>
public class Invoices : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetInvoices)
            .RequireAuthorization();

        groupBuilder.MapGet(GetInvoice, "{id:guid}")
            .RequireAuthorization();

        groupBuilder.MapPost(CreateInvoice)
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token

        groupBuilder.MapPost(RecordPayment, "{id:guid}/payments")
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token

        groupBuilder.MapPost(VoidInvoice, "{id:guid}/void")
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token
    }

    [EndpointSummary("List invoices")]
    [EndpointDescription("Returns invoices newest first, each with its lines and payments. Pass schoolId to list one school's. Master tenant only.")]
    public static async Task<Ok<IReadOnlyList<InvoiceDto>>> GetInvoices(ISender sender, Guid? schoolId)
    {
        return TypedResults.Ok(await sender.Send(new GetInvoicesQuery(schoolId)));
    }

    [EndpointSummary("Get an invoice")]
    [EndpointDescription("Returns one invoice with its lines and payments. Master tenant only.")]
    public static async Task<Ok<InvoiceDto>> GetInvoice(ISender sender, Guid id)
    {
        return TypedResults.Ok(await sender.Send(new GetInvoiceQuery(id)));
    }

    [EndpointSummary("Issue an invoice")]
    [EndpointDescription("Issues an invoice for the school's subscription and returns its id: for the current period, or the next one with nextPeriod true (the only choice while on trial). The amount is the subscription's price and the tax rate comes from the country's price book. One open invoice per subscription. Master tenant only.")]
    public static async Task<Ok<Guid>> CreateInvoice(ISender sender, CreateInvoiceCommand command)
    {
        return TypedResults.Ok(await sender.Send(command));
    }

    [EndpointSummary("Record a payment")]
    [EndpointDescription("Records a manual payment (bank transfer, cash, cheque, wallet) for an open invoice and returns the payment's id. The amount must equal the invoice total. The invoice becomes Paid and the school's subscription is activated or extended. Master tenant only.")]
    public static async Task<Ok<Guid>> RecordPayment(ISender sender, Guid id, RecordPaymentCommand command)
    {
        // The route names the invoice; an id in the body is ignored.
        return TypedResults.Ok(await sender.Send(command with { InvoiceId = id }));
    }

    [EndpointSummary("Void an invoice")]
    [EndpointDescription("Cancels an open invoice; the row is kept and the period can be invoiced again. A paid invoice cannot be voided. Master tenant only.")]
    public static async Task<NoContent> VoidInvoice(ISender sender, Guid id)
    {
        await sender.Send(new VoidInvoiceCommand(id));
        return TypedResults.NoContent();
    }
}
