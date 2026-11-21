using EducationPortal.Domain.Common;

namespace EducationPortal.Domain.Entities;

/// <summary>
/// The customer that owns one or more schools (ADR-021). It owns what the customer buys: its subscription and modules,
/// and its invoices. There is exactly one master tenant, the platform owner: it is created only by seeding through
/// <see cref="CreateMaster"/>, no method can change <see cref="IsMaster"/>, and a unique index allows only one.
/// </summary>
public class Tenant : BaseSoftDeletableEntity
{
    public const int NameMaxLength = 200;

    private Tenant()
    {
        // For EF Core.
        Name = string.Empty;
    }

    private Tenant(string name, bool isMaster)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(name.Trim().Length, NameMaxLength, nameof(name));

        Name = name.Trim();
        IsMaster = isMaster;
    }

    /// <summary>Creates a customer tenant. The master tenant is created only through <see cref="CreateMaster"/>.</summary>
    public static Tenant Create(string name) => new(name, isMaster: false);

    /// <summary>Creates the master tenant. Only seeding calls this; there is no use case for it.</summary>
    public static Tenant CreateMaster(string name) => new(name, isMaster: true);

    /// <summary>The master tenant can never be switched off or deleted: the platform needs one.</summary>
    protected override bool CanBeDeactivated => !IsMaster;

    public string Name { get; private set; }

    /// <summary>True for the one master tenant. Set at creation and never changed.</summary>
    public bool IsMaster { get; private set; }
}
