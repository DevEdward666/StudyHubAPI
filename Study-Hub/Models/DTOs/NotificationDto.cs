using Study_Hub.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace Study_Hub.Models.DTOs
{
    public class NotificationDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public NotificationType Type { get; set; }
        public bool IsRead { get; set; }
        public string? Data { get; set; }
        public string? ActionUrl { get; set; }
        public NotificationPriority Priority { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? ReadAt { get; set; }
    }

    public class CreateNotificationDto
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        [StringLength(255)]
        public string Title { get; set; }

        [Required]
        [StringLength(1000)]
        public string Message { get; set; }

        public NotificationType Type { get; set; } = NotificationType.System;

        public string? Data { get; set; }

        [StringLength(500)]
        public string? ActionUrl { get; set; }

        public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;

        public DateTime? ExpiresAt { get; set; }
    }

    public class UpdateNotificationDto
    {
        public string? Title { get; set; }
        public string? Message { get; set; }
        public NotificationType? Type { get; set; }
        public string? Data { get; set; }
        public string? ActionUrl { get; set; }
        public NotificationPriority? Priority { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }

    public class MarkNotificationReadDto
    {
        [Required]
        public List<Guid> NotificationIds { get; set; } = new List<Guid>();
    }

    public class NotificationFilterDto
    {
        public NotificationType? Type { get; set; }
        public bool? IsRead { get; set; }
        public NotificationPriority? Priority { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class NotificationSummaryDto
    {
        public int TotalCount { get; set; }
        public int UnreadCount { get; set; }
        public int TodayCount { get; set; }
        public int HighPriorityCount { get; set; }
        public int CriticalCount { get; set; }
        public Dictionary<NotificationType, int> CountByType { get; set; } = new Dictionary<NotificationType, int>();
    }

    public class PaginatedNotificationsDto
    {
        public List<NotificationDto> Notifications { get; set; } = new List<NotificationDto>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }

    public class BulkNotificationDto
    {
        [Required]
        public List<Guid> UserIds { get; set; } = new List<Guid>();

        [Required]
        [StringLength(255)]
        public string Title { get; set; }

        [Required]
        [StringLength(1000)]
        public string Message { get; set; }

        public NotificationType Type { get; set; } = NotificationType.System;

        public string? Data { get; set; }

        [StringLength(500)]
        public string? ActionUrl { get; set; }

        public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;

        public DateTime? ExpiresAt { get; set; }
    }
}