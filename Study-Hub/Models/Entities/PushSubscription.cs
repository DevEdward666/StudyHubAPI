using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Study_Hub.Models.Entities
{
    [Table("push_subscriptions")]
    public class PushSubscription
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("user_id")]
        public Guid UserId { get; set; }

        [Required]
        [Column("endpoint")]
        [StringLength(500)]
        public string Endpoint { get; set; }

        [Column("p256dh")]
        [StringLength(200)]
        public string? P256dh { get; set; }

        [Column("auth")]
        [StringLength(200)]
        public string? Auth { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("user_agent")]
        [StringLength(500)]
        public string? UserAgent { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Column("last_used_at")]
        public DateTime? LastUsedAt { get; set; }

        // Navigation property
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}

