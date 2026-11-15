using EducationPortal.Domain.Common;
using EducationPortal.Domain.Enums;
using EducationPortal.Domain.ValueObjects;

namespace EducationPortal.Domain.Entities;

/// <summary>
/// A person of a school (the table <c>UserDetails</c>): a principal, a teacher or other staff, with the person's name and
/// contact. Owned by the school (<see cref="SchoolId"/>) and covered by the tenant query filter (SEC-AUTHZ-03). The
/// person is not yet linked to a sign-in account. A campus names its principal with one of these rows.
/// </summary>
public class UserDetails : BaseSoftDeletableEntity
{
    public const int NameMaxLength = 100;

    private UserDetails()
    {
        // For EF Core.
        FirstName = string.Empty;
        LastName = string.Empty;
    }

    private UserDetails(int schoolId, string firstName, string? middleName, string lastName, UserType userType, string? phone)
    {
        SchoolId = schoolId;
        FirstName = firstName;
        MiddleName = middleName;
        LastName = lastName;
        UserType = userType;
        Phone = phone;
    }

    /// <summary>
    /// Creates a person of the school. The first and last names are required; the middle name is optional. Every name is
    /// trimmed and at most 100 characters; a blank middle name is stored as <c>null</c>.
    /// </summary>
    public static UserDetails Create(int schoolId, string firstName, string? middleName, string lastName, UserType userType, string? phone)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(schoolId);
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(firstName.Trim().Length, NameMaxLength, nameof(firstName));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(lastName.Trim().Length, NameMaxLength, nameof(lastName));

        var trimmedMiddle = middleName?.Trim();
        if (!string.IsNullOrEmpty(trimmedMiddle))
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan(trimmedMiddle.Length, NameMaxLength, nameof(middleName));
        }

        if (!Enum.IsDefined(userType))
        {
            throw new ArgumentOutOfRangeException(nameof(userType));
        }

        var trimmedPhone = phone?.Trim();
        if (!string.IsNullOrEmpty(trimmedPhone) && !ContactRules.IsValidPhone(trimmedPhone))
        {
            throw new ArgumentException("A phone number has 7 to 15 digits and an optional leading +.", nameof(phone));
        }

        return new UserDetails(
            schoolId,
            firstName.Trim(),
            string.IsNullOrEmpty(trimmedMiddle) ? null : trimmedMiddle,
            lastName.Trim(),
            userType,
            string.IsNullOrEmpty(trimmedPhone) ? null : trimmedPhone);
    }

    public int SchoolId { get; private set; }

    public string FirstName { get; private set; }

    /// <summary>The middle name, when the person has one.</summary>
    public string? MiddleName { get; private set; }

    public string LastName { get; private set; }

    public UserType UserType { get; private set; }

    public string? Phone { get; private set; }
}
