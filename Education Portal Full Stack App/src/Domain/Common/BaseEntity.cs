using System.ComponentModel.DataAnnotations.Schema;

namespace EducationPortal.Domain.Common;

/// <summary>
/// Every entity has two identities (ENG-ID-01, ENG-ID-02):
/// <list type="bullet">
/// <item><see cref="Id"/> is the internal <c>long</c> (PostgreSQL <c>bigint</c>) primary key. Foreign keys, joins and
/// query filters use it. It never leaves the server: no DTO, route, query string or response carries it.</item>
/// <item><see cref="PublicId"/> is the <see cref="Guid"/> alternate key that the API exposes. Handlers resolve it to
/// <see cref="Id"/> before any business logic.</item>
/// </list>
/// </summary>
public abstract class BaseEntity
{
    public long Id { get; set; }

    /// <summary>
    /// The identity the API exposes. Version 7 (time-ordered) so the unique index stays compact as rows are added. Set
    /// once when the object is created and never changed.
    /// </summary>
    public Guid PublicId { get; private set; } = Guid.CreateVersion7();

    private readonly List<BaseEvent> _domainEvents = new();

    [NotMapped]
    public IReadOnlyCollection<BaseEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(BaseEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void RemoveDomainEvent(BaseEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
