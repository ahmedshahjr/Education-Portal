namespace EducationPortal.Domain.Common;

/// <summary>
/// An entity that is never removed from the database. <see cref="IsDeleted"/> hides it from every query (the global
/// query filter in <c>ApplicationDbContext</c>), and it stays in the table for audit and recovery. <see cref="IsActive"/>
/// is an administrator's switch: an inactive entity is still listed for its administrators, but its tenant is refused
/// at the request level.
/// </summary>
public abstract class BaseSoftDeletableEntity : BaseAuditableEntity
{
    public bool IsActive { get; private set; } = true;

    public bool IsDeleted { get; private set; }

    public DateTimeOffset? DeletedAt { get; private set; }

    public string? DeletedBy { get; private set; }

    /// <summary>Whether this entity may be switched off or deleted. Subclasses protect what must never go away.</summary>
    protected virtual bool CanBeDeactivated => true;

    public void Deactivate()
    {
        if (IsDeleted)
        {
            throw new InvalidOperationException("A deleted entity cannot be changed.");
        }

        if (!CanBeDeactivated)
        {
            throw new InvalidOperationException("This entity cannot be deactivated.");
        }

        IsActive = false;
    }

    public void Activate()
    {
        if (IsDeleted)
        {
            throw new InvalidOperationException("A deleted entity cannot be changed.");
        }

        IsActive = true;
    }

    /// <summary>Soft-deletes the entity. Deleting twice keeps the first deletion time and user.</summary>
    public void Delete(DateTimeOffset deletedAt, string? deletedBy)
    {
        if (!CanBeDeactivated)
        {
            throw new InvalidOperationException("This entity cannot be deleted.");
        }

        if (IsDeleted)
        {
            return;
        }

        IsDeleted = true;
        IsActive = false;
        DeletedAt = deletedAt;
        DeletedBy = deletedBy;
    }
}
