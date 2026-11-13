namespace EducationPortal.Domain.Entities;

/// <summary>
/// A school administrator's request to add a paid module to the school's subscription. It changes nothing by itself:
/// the master tenant approves it (the module is then added to the subscription) or rejects it. Owned by the school
/// (<c>SchoolId</c>) and covered by the tenant query filter (SEC-AUTHZ-03). A school has at most one pending request
/// per module.
/// </summary>
public class AddOnRequest : BaseAuditableEntity
{
    private AddOnRequest()
    {
        // For EF Core.
    }

    private AddOnRequest(int schoolId, PortalModule module)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(schoolId);
        ArgumentNullException.ThrowIfNull(module);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(module.Id);

        if (module.IsCore)
        {
            throw new ArgumentException("Core modules are in every subscription and are not requested.", nameof(module));
        }

        SchoolId = schoolId;
        ModuleId = module.Id;
        Status = AddOnRequestStatus.Pending;
    }

    public static AddOnRequest Create(int schoolId, PortalModule module) => new(schoolId, module);

    public int SchoolId { get; private set; }

    public int ModuleId { get; private set; }

    public AddOnRequestStatus Status { get; private set; }

    /// <summary>When the master tenant decided, or <c>null</c> while pending.</summary>
    public DateTimeOffset? DecidedAt { get; private set; }

    public void Approve(DateTimeOffset now) => Decide(AddOnRequestStatus.Approved, now);

    public void Reject(DateTimeOffset now) => Decide(AddOnRequestStatus.Rejected, now);

    private void Decide(AddOnRequestStatus status, DateTimeOffset now)
    {
        if (Status != AddOnRequestStatus.Pending)
        {
            throw new InvalidOperationException("Only a pending request can be decided.");
        }

        Status = status;
        DecidedAt = now;
    }
}
