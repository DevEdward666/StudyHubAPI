using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Study_Hub.Models.Entities
{
    public enum SubscriptionStatus
    {
        Active,
        Expired,
        Cancelled,
        Suspended
    }

    [Table("user_subscriptions")]
    public class UserSubscription
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("user_id")]
        public Guid UserId { get; set; }

        [Required]
        [Column("package_id")]
        public Guid PackageId { get; set; }

        [Required]
        [Column("total_hours")]
        public decimal TotalHours { get; set; } // Total hours purchased

        [Required]
        [Column("remaining_hours")]
        public decimal RemainingHours { get; set; } // Hours left

        [Column("hours_used")]
        public decimal HoursUsed { get; set; } = 0; // Hours consumed

        [Required]
        [Column("purchase_date")]
        public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;

        [Column("activation_date")]
        public DateTime? ActivationDate { get; set; } // When first session started

        [Column("expiry_date")]
        public DateTime? ExpiryDate { get; set; } // Optional expiry date

        [Required]
        [Column("status")]
        [StringLength(50)]
        public string Status { get; set; } = "Active"; // Active, Expired, Cancelled, Suspended

        [Column("purchase_amount", TypeName = "decimal(10,2)")]
        public decimal PurchaseAmount { get; set; }

        [Column("payment_method")]
        [StringLength(50)]
        public string? PaymentMethod { get; set; }

        [Column("transaction_reference")]
        [StringLength(255)]
        public string? TransactionReference { get; set; }

        [Column("notes")]
        [StringLength(1000)]
        public string? Notes { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Column("created_by")]
        public Guid? CreatedBy { get; set; } // Admin who created this subscription

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("PackageId")]
        public virtual SubscriptionPackage Package { get; set; }

        public virtual ICollection<TableSession> Sessions { get; set; } = new List<TableSession>();
    }
}

