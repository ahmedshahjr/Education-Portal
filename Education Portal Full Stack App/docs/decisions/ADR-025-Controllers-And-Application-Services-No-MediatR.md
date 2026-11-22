# ADR-025: Controllers and Application Services, No MediatR

## Status

Accepted by the maintainer on 2026-11-21. Supersedes [ADR-003](ADR-003-MediatR-Contracts-In-Domain.md) and the MediatR
parts of AGENTS.md section 3 and section 6. The decision below was refined while it was implemented (2026-11-22): the
request rules are an explicit call in each action, not a filter (see decision 3).

## Context

Every use case was an `IRequest` handled by an `IRequestHandler`, sent by minimal API endpoints through `ISender`. A
MediatR pipeline ran the cross-cutting rules for every request: logging, unhandled-exception logging, authorisation
(login, password change, master tenant, module, role, rights, policy), FluentValidation, and performance logging. Only
one handler called another (SavePersonAccess). Domain events were scaffolding that nothing subscribed to. The maintainer
asked to remove MediatR and use controllers directly.

## Decision

1. **No mediator library.** Each use case is a plain service class in `src/Application/<Feature>/`, the existing
   `...CommandHandler` / `...QueryHandler` class without its MediatR interface, with one public `Handle(request, ct)`
   method. A request is a plain record (the DTO), as today. Every service class ending in `Handler` is registered in DI by
   convention (`Application/DependencyInjection.cs`), so a controller injects it directly.
2. **Controllers call services.** MVC controllers in `src/Web/Controllers/<Feature>Controller.cs` derive from
   `ApiControllerBase` (`[ApiController]`, model-state errors go through `ProblemDetails`). Routes stay `/api/{Name}`,
   action names stay the old endpoint method names, and `ControllerOperationIdTransformer` gives each action its method
   name as the OpenAPI operation id and the controller name as the tag, so the generated Angular client does not change.
3. **The request rules are explicit, and the cross-cutting logging is a filter.**
   - `IRequestRules.CheckAsync(request, ct)` (Application) applies the sign-in, password-change, master-tenant, module,
     role, right and policy checks (`IRequestAuthorizer`, read from `[Authorize]` on the request type) and then the
     request's FluentValidation validators. Each action calls it with **the request it is about to run**, after building
     the final request (route ids included). A filter cannot do this: it sees only the bound body, so a route id added
     with `with { ... }`, or a request built from route values, would be checked too early or not at all.
   - A use case that calls another use case checks the inner request too (`SavePersonAccessCommandHandler`), so the
     rules of the inner call still apply inside the transaction, as they did through MediatR.
   - Global filters (`Web/Filters`, order in `FilterOrder`): `LoggingFilter` and `PerformanceFilter`
     (the same log events and the 500 ms warning as before), `UnhandledExceptionFilter`, and the antiforgery check
     `[ValidateAntiforgeryToken]` on each unsafe action.
   - `ProblemDetailsExceptionHandler` is unchanged, so every error keeps its status and body.
4. **Use cases call use cases through services.** A service that needs another use case injects that service and calls
   its `Handle` after `IRequestRules.CheckAsync`. The transaction helper (`ExecuteInTransactionAsync`) keeps working
   unchanged.
5. **Domain events** are removed with MediatR: `BaseEvent` stops implementing `INotification`, and the
   `DispatchDomainEventsInterceptor` that published them is deleted (no subscriber ever existed).

## Consequences

- Easier: each feature is a plain class with a plain test; no runtime reflection over handlers. The request checks are
  visible in each action.
- Harder: every new action must call `IRequestRules.CheckAsync` with its final request. The denial tests of every use
  case (other school, other campus, module off) are the proof, and a removed check must make one of them fail.
- Route-id requests: the check runs after the route id is merged, so validators that require the id (for example
  `CampusId NotEmpty`) keep working.
- `[Consumes]` is not used on the controllers: it is an action constraint and made GET requests without a content type
  return 404. The JSON-only media types MVC adds are removed by `ControllerOperationIdTransformer` instead.
- Query parameters that the minimal API required (non-nullable values without a default) need `[BindRequired]`; the
  base controller turns an invalid model state into the same ProblemDetails.
- The OpenAPI document must not change its operation ids, paths, schemas or tags. A parity check against the generated
  `src/Web/openapi/v1.json` from before the change is the test of this decision.
