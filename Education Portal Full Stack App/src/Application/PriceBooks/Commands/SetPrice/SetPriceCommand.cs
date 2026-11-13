using System.Globalization;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.PriceBooks.Commands.SetPrice;

/// <summary>
/// Sets the price of a plan or of a paid add-on module for one billing cycle in a price book: adds it, or replaces the
/// existing price. Give <see cref="PlanCode"/> or <see cref="ModuleCode"/>, not both. Core modules are never priced.
/// Master tenant administrators only (SEC-AUTHZ-07); audited (SEC-LOG-03).
/// </summary>
[Authorize(Roles = Roles.Administrator, MasterTenant = true)]
public record SetPriceCommand : IRequest
{
    /// <summary>The price book's public id, taken from the route.</summary>
    public Guid PriceBookId { get; init; }

    public string? PlanCode { get; init; }

    public string? ModuleCode { get; init; }

    public BillingCycle BillingCycle { get; init; }

    public decimal Price { get; init; }
}

public class SetPriceCommandValidator : AbstractValidator<SetPriceCommand>
{
    public SetPriceCommandValidator(IApplicationDbContext context)
    {
        RuleFor(c => c.PriceBookId).NotEmpty();
        RuleFor(c => c.BillingCycle).IsInEnum();
        RuleFor(c => c.Price).Must(PriceBookItem.IsValidPrice).WithMessage(_ => ValidationMessages.PriceInvalid);

        RuleFor(c => c)
            .Must(c => string.IsNullOrWhiteSpace(c.PlanCode) != string.IsNullOrWhiteSpace(c.ModuleCode))
            .WithMessage(_ => ValidationMessages.PriceTargetRequired)
            .OverridePropertyName(nameof(SetPriceCommand.PlanCode));

        RuleFor(c => c.ModuleCode)
            .Cascade(CascadeMode.Stop)
            .MustAsync(async (code, cancellationToken) => await context.Modules.AnyAsync(m => m.Code == code, cancellationToken))
            .WithMessage(_ => ValidationMessages.ModuleNotFound)
            .MustAsync(async (code, cancellationToken) => !await context.Modules.AnyAsync(m => m.Code == code && m.IsCore, cancellationToken))
            .WithMessage(_ => ValidationMessages.CoreModuleNotSold)
            .When(c => !string.IsNullOrWhiteSpace(c.ModuleCode));

        RuleFor(c => c.PlanCode)
            .MustAsync(async (code, cancellationToken) => await context.Plans.AnyAsync(p => p.Code == code, cancellationToken))
            .WithMessage(_ => ValidationMessages.PlanNotFound)
            .When(c => !string.IsNullOrWhiteSpace(c.PlanCode));
    }
}

public class SetPriceCommandHandler : IRequestHandler<SetPriceCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;

    public SetPriceCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog)
    {
        _context = context;
        _auditLog = auditLog;
    }

    public async Task Handle(SetPriceCommand request, CancellationToken cancellationToken)
    {
        var book = await _context.PriceBooks.FirstOrDefaultAsync(b => b.PublicId == request.PriceBookId, cancellationToken);
        Guard.Against.NotFound(request.PriceBookId, book);

        PriceBookItem? item;
        if (!string.IsNullOrWhiteSpace(request.PlanCode))
        {
            var plan = await _context.Plans.FirstAsync(p => p.Code == request.PlanCode, cancellationToken);
            item = await _context.PriceBookItems.FirstOrDefaultAsync(
                i => i.PriceBookId == book.Id && i.PlanId == plan.Id && i.BillingCycle == request.BillingCycle, cancellationToken);
            item?.SetPrice(request.Price);
            item ??= _context.PriceBookItems.Add(PriceBookItem.ForPlan(book, plan, request.BillingCycle, request.Price)).Entity;
        }
        else
        {
            var module = await _context.Modules.FirstAsync(m => m.Code == request.ModuleCode, cancellationToken);
            item = await _context.PriceBookItems.FirstOrDefaultAsync(
                i => i.PriceBookId == book.Id && i.ModuleId == module.Id && i.BillingCycle == request.BillingCycle, cancellationToken);
            item?.SetPrice(request.Price);
            item ??= _context.PriceBookItems.Add(PriceBookItem.ForModule(book, module, request.BillingCycle, request.Price)).Entity;
        }

        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.PriceChanged,
            SecurityAuditOutcome.Success,
            targetType: nameof(PriceBookItem),
            targetId: item.Id.ToString(CultureInfo.InvariantCulture));
    }
}
