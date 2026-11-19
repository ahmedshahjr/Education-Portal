using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.Calendar.Common;

/// <summary>A school event as the calendar shows it, in UTC.</summary>
public sealed record SchoolEventDto(Guid Id, string Title, string? Description, DateTimeOffset StartsAt, DateTimeOffset EndsAt, EventAudience Audience);

/// <summary>One notification of the signed-in account.</summary>
public sealed record NotificationDto(Guid Id, string Title, string? Body, DateTimeOffset Created, DateTimeOffset? ReadAt);

/// <summary>The newest notifications of the signed-in account, and how many are unread.</summary>
public sealed record NotificationsDto(IReadOnlyList<NotificationDto> Items, int UnreadCount);
