using EducationPortal.Application.PriceBooks.Commands.CreatePriceBook;
using EducationPortal.Application.PriceBooks.Commands.DeletePriceBook;
using EducationPortal.Application.PriceBooks.Commands.RemovePrice;
using EducationPortal.Application.PriceBooks.Commands.SetPrice;
using EducationPortal.Application.PriceBooks.Commands.UpdatePriceBook;
using EducationPortal.Application.PriceBooks.Common;
using EducationPortal.Application.PriceBooks.Queries.GetPriceBook;
using EducationPortal.Application.PriceBooks.Queries.GetPriceBooks;
using EducationPortal.Domain.Enums;
using EducationPortal.Web.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http.HttpResults;

namespace EducationPortal.Web.Endpoints;

/// <summary>
/// The price books: one per country, in one currency, pricing plans and paid add-on modules per billing cycle. Master
/// tenant administrators only (SEC-AUTHZ-07).
/// </summary>
public class PriceBooks : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetPriceBooks)
            .RequireAuthorization();

        groupBuilder.MapGet(GetPriceBook, "{id:guid}")
            .RequireAuthorization();

        groupBuilder.MapPost(CreatePriceBook)
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token

        groupBuilder.MapPut(UpdatePriceBook, "{id:guid}")
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token

        groupBuilder.MapDelete(DeletePriceBook, "{id:guid}")
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token

        groupBuilder.MapPut(SetPrice, "{id:guid}/prices")
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token

        groupBuilder.MapDelete(RemovePrice, "{id:guid}/prices")
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token
    }

    [EndpointSummary("List price books")]
    [EndpointDescription("Returns the price books by country, each with its currency and every price (plan or module code, billing cycle, price). Master tenant only.")]
    public static async Task<Ok<IReadOnlyList<PriceBookDto>>> GetPriceBooks(ISender sender)
    {
        return TypedResults.Ok(await sender.Send(new GetPriceBooksQuery()));
    }

    [EndpointSummary("Get a price book")]
    [EndpointDescription("Returns one price book with its prices. Master tenant only.")]
    public static async Task<Ok<PriceBookDto>> GetPriceBook(ISender sender, Guid id)
    {
        return TypedResults.Ok(await sender.Send(new GetPriceBookQuery(id)));
    }

    [EndpointSummary("Add a price book")]
    [EndpointDescription("Adds an empty price book for a country (ISO 3166-1 alpha-2, for example PK) in a currency (ISO 4217, for example PKR) and returns its id. A country has one price book. Master tenant only.")]
    public static async Task<Ok<Guid>> CreatePriceBook(ISender sender, CreatePriceBookCommand command)
    {
        return TypedResults.Ok(await sender.Send(command));
    }

    [EndpointSummary("Change a price book")]
    [EndpointDescription("Sets the price book's currency and active switch. Changing the currency does not convert the prices. Master tenant only.")]
    public static async Task<NoContent> UpdatePriceBook(ISender sender, Guid id, UpdatePriceBookCommand command)
    {
        // The route names the price book; an id in the body is ignored.
        await sender.Send(command with { Id = id });

        return TypedResults.NoContent();
    }

    [EndpointSummary("Delete a price book")]
    [EndpointDescription("Deletes a price book and its prices. The row stays for audit and the country can get a new price book. Master tenant only.")]
    public static async Task<NoContent> DeletePriceBook(ISender sender, Guid id)
    {
        await sender.Send(new DeletePriceBookCommand(id));

        return TypedResults.NoContent();
    }

    [EndpointSummary("Set a price")]
    [EndpointDescription("Sets the price of a plan (planCode) or of a paid add-on module (moduleCode), not both, for one billing cycle (Monthly or Yearly), in the price book's currency. Adds the price or replaces the existing one. Core modules cannot be priced. Master tenant only.")]
    public static async Task<NoContent> SetPrice(ISender sender, Guid id, SetPriceCommand command)
    {
        // The route names the price book; an id in the body is ignored.
        await sender.Send(command with { PriceBookId = id });

        return TypedResults.NoContent();
    }

    [EndpointSummary("Remove a price")]
    [EndpointDescription("Removes the price of a plan (planCode) or module (moduleCode) for one billing cycle. Answers 404 when there is no such price. Master tenant only.")]
    public static async Task<NoContent> RemovePrice(ISender sender, Guid id, string? planCode, string? moduleCode, BillingCycle billingCycle)
    {
        await sender.Send(new RemovePriceCommand(id, planCode, moduleCode, billingCycle));

        return TypedResults.NoContent();
    }
}
