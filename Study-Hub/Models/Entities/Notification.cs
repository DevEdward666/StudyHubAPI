using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Study_Hub.Models.Entities
{
    [Table("notifications")]
    public class Notification
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("user_id")]
        public Guid UserId { get; set; }

        [Required]
        [Column("title")]
        [StringLength(255)]
        public string Title { get; set; }

        [Required]
        [Column("message")]
        [StringLength(1000)]
        public string Message { get; set; }

        [Column("type")]
        public NotificationType Type { get; set; }

        [Column("is_read")]
        public bool IsRead { get; set; } = false;

        [Column("data")]
        public string? Data { get; set; } // JSON string for additional data

        [Column("action_url")]
        [StringLength(500)]
        public string? ActionUrl { get; set; }

        [Column("priority")]
        public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;

        [Column("expires_at")]
        public DateTime? ExpiresAt { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Column("read_at")]
        public DateTime? ReadAt { get; set; }

        // Navigation property
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }

    public enum NotificationType
    {
        System = 0,
        Credit = 1,
        Session = 2,
        Payment = 3,
        Booking = 4,
        Reminder = 5,
        Promotion = 6,
        Alert = 7
    }

    public enum NotificationPriority
    {
        Low = 0,
        Normal = 1,
        High = 2,
        Critical = 3
    }
}