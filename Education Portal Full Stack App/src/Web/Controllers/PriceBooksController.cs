using EducationPortal.Application.Common.Security;
using EducationPortal.Application.PriceBooks.Commands.CreatePriceBook;
using EducationPortal.Application.PriceBooks.Commands.DeletePriceBook;
using EducationPortal.Application.PriceBooks.Commands.RemovePrice;
using EducationPortal.Application.PriceBooks.Commands.SetPrice;
using EducationPortal.Application.PriceBooks.Commands.UpdatePriceBook;
using EducationPortal.Application.PriceBooks.Common;
using EducationPortal.Application.PriceBooks.Queries.GetPriceBook;
using EducationPortal.Application.PriceBooks.Queries.GetPriceBooks;
using EducationPortal.Domain.Enums;
using EducationPortal.Web.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace EducationPortal.Web.Controllers;

/// <summary>
/// The price books: one per country, in one currency, pricing plans and paid add-on modules per billing cycle. Master
/// tenant administrators only (SEC-AUTHZ-07).
/// </summary>
[Route("api/PriceBooks")]
[Microsoft.AspNetCore.Authorization.Authorize]
public class PriceBooksController(
    IRequestRules rules,
    GetPriceBooksQueryHandler getPriceBooks,
    GetPriceBookQueryHandler getPriceBook,
    CreatePriceBookCommandHandler createPriceBook,
    UpdatePriceBookCommandHandler updatePriceBook,
    DeletePriceBookCommandHandler deletePriceBook,
    SetPriceCommandHandler setPrice,
    RemovePriceCommandHandler removePrice) : ApiControllerBase
{
    [HttpGet]
    [EndpointSummary("List price books")]
    [EndpointDescription("Returns the price books by country, each with its currency and every price (plan or module code, billing cycle, price). Master tenant only.")]
    public async Task<ActionResult<IReadOnlyList<PriceBookDto>>> GetPriceBooks(CancellationToken cancellationToken)
    {
        var query = new GetPriceBooksQuery();
        await rules.CheckAsync(query, cancellationToken);
        return Ok(await getPriceBooks.Handle(query, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [EndpointSummary("Get a price book")]
    [EndpointDescription("Returns one price book with its prices. Master tenant only.")]
    public async Task<ActionResult<PriceBookDto>> GetPriceBook(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetPriceBookQuery(id);
        await rules.CheckAsync(query, cancellationToken);
        return Ok(await getPriceBook.Handle(query, cancellationToken));
    }

    [HttpPost]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [EndpointSummary("Add a price book")]
    [EndpointDescription("Adds an empty price book for a country (ISO 3166-1 alpha-2, for example PK) in a currency (ISO 4217, for example PKR) and returns its id. A country has one price book. Master tenant only.")]
    public async Task<ActionResult<Guid>> CreatePriceBook(CreatePriceBookCommand command, CancellationToken cancellationToken)
    {
        await rules.CheckAsync(command, cancellationToken);
        return Ok(await createPriceBook.Handle(command, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [EndpointSummary("Change a price book")]
    [EndpointDescription("Sets the price book's currency and active switch. Changing the currency does not convert the prices. Master tenant only.")]
    public async Task<IActionResult> UpdatePriceBook(Guid id, UpdatePriceBookCommand command, CancellationToken cancellationToken)
    {
        // The route names the price book; an id in the body is ignored.
        var request = command with { Id = id };
        await rules.CheckAsync(request, cancellationToken);
        await updatePriceBook.Handle(request, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [EndpointSummary("Delete a price book")]
    [EndpointDescription("Deletes a price book and its prices. The row stays for audit and the country can get a new price book. Master tenant only.")]
    public async Task<IActionResult> DeletePriceBook(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeletePriceBookCommand(id);
        await rules.CheckAsync(command, cancellationToken);
        await deletePriceBook.Handle(command, cancellationToken);
        return NoContent();
    }

    [HttpPut("{id:guid}/prices")]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [EndpointSummary("Set a price")]
    [EndpointDescription("Sets the price of a plan (planCode) or of a paid add-on module (moduleCode), not both, for one billing cycle (Monthly or Yearly), in the price book's currency. Adds the price or replaces the existing one. Core modules cannot be priced. Master tenant only.")]
    public async Task<IActionResult> SetPrice(Guid id, SetPriceCommand command, CancellationToken cancellationToken)
    {
        // The route names the price book; an id in the body is ignored.
        var request = command with { PriceBookId = id };
        await rules.CheckAsync(request, cancellationToken);
        await setPrice.Handle(request, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}/prices")]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [EndpointSummary("Remove a price")]
    [EndpointDescription("Removes the price of a plan (planCode) or module (moduleCode) for one billing cycle. Answers 404 when there is no such price. Master tenant only.")]
    public async Task<IActionResult> RemovePrice(
        Guid id, string? planCode, ModuleCode? moduleCode, [BindRequired] BillingCycle billingCycle, CancellationToken cancellationToken)
    {
        var command = new RemovePriceCommand(id, planCode, moduleCode, billingCycle);
        await rules.CheckAsync(command, cancellationToken);
        await removePrice.Handle(command, cancellationToken);
        return NoContent();
    }
}
