using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Study_Hub.Models.Entities
{
    public enum PackageType
    {
        Hourly,
        Daily,
        Weekly,
        Monthly
    }

    [Table("subscription_packages")]
    public class SubscriptionPackage
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("name")]
        [StringLength(100)]
        public string Name { get; set; } // e.g., "1 Week Package", "2 Months Package"

        [Required]
        [Column("package_type")]
        [StringLength(50)]
        public string PackageType { get; set; } // Hourly, Daily, Weekly, Monthly

        [Required]
        [Column("duration_value")]
        public int DurationValue { get; set; } // 1, 2, 3, etc.

        [Required]
        [Column("total_hours")]
        public decimal TotalHours { get; set; } // Total hours in this package (e.g., 168 for 1 week)

        [Required]
        [Column("price", TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [Column("description")]
        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("display_order")]
        public int DisplayOrder { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Column("created_by")]
        public Guid? CreatedBy { get; set; }

        // Navigation properties
        public virtual ICollection<UserSubscription> UserSubscriptions { get; set; } = new List<UserSubscription>();
    }
}

