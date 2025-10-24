namespace Study_Hub.Models.DTOs
{
    // Request DTOs
    public class PushSubscriptionDto
    {
        public string Endpoint { get; set; }
        public PushSubscriptionKeys? Keys { get; set; }
        public string? UserAgent { get; set; }
    }

    public class PushSubscriptionKeys
    {
        public string P256dh { get; set; }
        public string Auth { get; set; }
    }

    // Response DTOs
    public class PushSubscriptionResponseDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Endpoint { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUsedAt { get; set; }
    }

    // Push Message DTOs
    public class PushNotificationDto
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public string? Icon { get; set; }
        public string? Badge { get; set; }
        public string? Image { get; set; }
        public string? Tag { get; set; }
        public string? Url { get; set; }
        public Dictionary<string, object>? Data { get; set; }
        public List<NotificationAction>? Actions { get; set; }
    }

    public class NotificationAction
    {
        public string Action { get; set; }
        public string Title { get; set; }
        public string? Icon { get; set; }
    }

    // VAPID Keys DTO
    public class VapidPublicKeyDto
    {
        public string PublicKey { get; set; }
    }

    // Test Push DTO
    public class TestPushDto
    {
        public Guid UserId { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
    }
}

