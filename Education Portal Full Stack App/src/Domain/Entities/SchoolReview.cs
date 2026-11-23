using EducationPortal.Domain.Common;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Domain.Entities;

/// <summary>
/// A visitor's review of a school (the table <c>SchoolReviews</c>). A visitor needs no account, so a review starts
/// <see cref="ReviewStatus.Pending"/> and is public only once the school approves it. The school may reply to an approved
/// review. A removed review stays in the table (soft delete).
/// </summary>
public class SchoolReview : BaseSoftDeletableEntity, ITenantOwned
{
    public const int MinimumRating = 1;
    public const int MaximumRating = 5;
    public const int NameMaxLength = 100;
    public const int TitleMaxLength = 100;
    public const int BodyMaxLength = 2000;
    public const int ReplyMaxLength = 1000;

    private SchoolReview()
    {
        // For EF Core.
        ReviewerName = string.Empty;
        Body = string.Empty;
    }

    /// <summary>Creates a pending review. The rating is 1 to 5, and the name and body are required.</summary>
    public static SchoolReview Submit(int schoolId, int campusId, string reviewerName, ReviewerRelation relation, int rating, string? title, string body, DateTimeOffset now)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(schoolId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(campusId);

        if (!Enum.IsDefined(relation))
        {
            throw new ArgumentOutOfRangeException(nameof(relation));
        }

        ArgumentOutOfRangeException.ThrowIfLessThan(rating, MinimumRating);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(rating, MaximumRating);

        ArgumentException.ThrowIfNullOrWhiteSpace(reviewerName);
        ArgumentException.ThrowIfNullOrWhiteSpace(body);
        var name = reviewerName.Trim();
        var text = body.Trim();
        var heading = title?.Trim();
        ArgumentOutOfRangeException.ThrowIfGreaterThan(name.Length, NameMaxLength, nameof(reviewerName));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(text.Length, BodyMaxLength, nameof(body));
        if (!string.IsNullOrEmpty(heading))
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan(heading.Length, TitleMaxLength, nameof(title));
        }

        return new SchoolReview
        {
            SchoolId = schoolId,
            CampusId = campusId,
            ReviewerName = name,
            Relation = relation,
            Rating = rating,
            Title = string.IsNullOrEmpty(heading) ? null : heading,
            Body = text,
            Status = ReviewStatus.Pending,
            SubmittedAt = now,
        };
    }

    public int SchoolId { get; private set; }

    public int CampusId { get; private set; }

    public string ReviewerName { get; private set; }

    public ReviewerRelation Relation { get; private set; }

    public int Rating { get; private set; }

    public string? Title { get; private set; }

    public string Body { get; private set; }

    public ReviewStatus Status { get; private set; }

    public DateTimeOffset SubmittedAt { get; private set; }

    public DateTimeOffset? ModeratedAt { get; private set; }

    /// <summary>The user id (not a name) of whoever approved or rejected the review.</summary>
    public string? ModeratedBy { get; private set; }

    public string? Reply { get; private set; }

    public DateTimeOffset? RepliedAt { get; private set; }

    public void Approve(string moderatedBy, DateTimeOffset now) => Moderate(ReviewStatus.Approved, moderatedBy, now);

    public void Reject(string moderatedBy, DateTimeOffset now) => Moderate(ReviewStatus.Rejected, moderatedBy, now);

    /// <summary>The school answers an approved review. A second reply replaces the first.</summary>
    public void ReplyTo(string reply, DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reply);

        if (Status != ReviewStatus.Approved)
        {
            throw new InvalidOperationException("Only an approved review can be answered.");
        }

        var trimmed = reply.Trim();
        ArgumentOutOfRangeException.ThrowIfGreaterThan(trimmed.Length, ReplyMaxLength, nameof(reply));

        Reply = trimmed;
        RepliedAt = now;
    }

    private void Moderate(ReviewStatus decision, string moderatedBy, DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(moderatedBy);

        if (Status != ReviewStatus.Pending)
        {
            throw new InvalidOperationException("Only a pending review can be approved or rejected.");
        }

        Status = decision;
        ModeratedBy = moderatedBy;
        ModeratedAt = now;
    }
}
