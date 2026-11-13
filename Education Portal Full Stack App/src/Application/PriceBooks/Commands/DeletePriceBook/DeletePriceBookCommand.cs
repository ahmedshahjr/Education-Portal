using System.Globalization;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.PriceBooks.Commands.DeletePriceBook;

/// <summary>
/// Deletes a price book (soft delete: the row stays for audit) and removes its prices. The country can then get a new
/// price book. Master tenant administrators only (SEC-AUTHZ-07); audited (SEC-LOG-03).
/// </summary>
[Authorize(Roles = Roles.Administrator, MasterTenant = true)]
public record DeletePriceBookCommand(Guid Id) : IRequest;

public class DeletePriceBookCommandHandler : IRequestHandler<DeletePriceBookCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;
    private readonly IUser _user;
    private readonly TimeProvider _timeProvider;

    public DeletePriceBookCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog, IUser user, TimeProvider timeProvider)
    {
        _context = context;
        _auditLog = auditLog;
        _user = user;
        _timeProvider = timeProvider;
    }

    public async Task Handle(DeletePriceBookCommand request, CancellationToken cancellationToken)
    {
        var book = await _context.PriceBooks.FirstOrDefaultAsync(b => b.PublicId == request.Id, cancellationToken);
        Guard.Against.NotFound(request.Id, book);

        var prices = await _context.PriceBookItems.Where(i => i.PriceBookId == book.Id).ToListAsync(cancellationToken);
        _context.PriceBookItems.RemoveRange(prices);

        book.Delete(_timeProvider.GetUtcNow(), _user.Id);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.PriceBookDeleted,
            SecurityAuditOutcome.Success,
            targetType: nameof(PriceBook),
            targetId: book.Id.ToString(CultureInfo.InvariantCulture));
    }
}
