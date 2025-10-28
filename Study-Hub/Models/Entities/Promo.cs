using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Study_Hub.Models.Entities
{
    public enum PromoType
    {
        Percentage,     // e.g., 20% bonus credits
        FixedAmount,    // e.g., +50 credits bonus
        BuyXGetY        // e.g., Buy 100, Get 20 free
    }

    public enum PromoStatus
    {
        Active,
        Inactive,
        Expired,
        Scheduled
    }

    [Table("promos")]
    public class Promo
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("code")]
        [StringLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [Column("name")]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Column("description")]
        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [Column("type")]
        public PromoType Type { get; set; }

        [Required]
        [Column("status")]
        public PromoStatus Status { get; set; } = PromoStatus.Active;

        // Discount/Bonus values
        [Column("percentage_bonus", TypeName = "decimal(5,2)")]
        public decimal? PercentageBonus { get; set; } // For Percentage type

        [Column("fixed_bonus_amount", TypeName = "decimal(10,2)")]
        public decimal? FixedBonusAmount { get; set; } // For FixedAmount type

        [Column("buy_amount", TypeName = "decimal(10,2)")]
        public decimal? BuyAmount { get; set; } // For BuyXGetY type

        [Column("get_amount", TypeName = "decimal(10,2)")]
        public decimal? GetAmount { get; set; } // For BuyXGetY type

        // Constraints
        [Column("min_purchase_amount", TypeName = "decimal(10,2)")]
        public decimal? MinPurchaseAmount { get; set; }

        [Column("max_discount_amount", TypeName = "decimal(10,2)")]
        public decimal? MaxDiscountAmount { get; set; }

        [Column("usage_limit")]
        public int? UsageLimit { get; set; } // Total uses allowed

        [Column("usage_per_user")]
        public int? UsagePerUser { get; set; } // Max uses per user

        [Column("current_usage_count")]
        public int CurrentUsageCount { get; set; } = 0;

        // Validity
        [Column("start_date")]
        public DateTime? StartDate { get; set; }

        [Column("end_date")]
        public DateTime? EndDate { get; set; }

        // Metadata
        [Column("created_by")]
        public Guid CreatedBy { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Column("is_deleted")]
        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        [ForeignKey("CreatedBy")]
        public virtual User? Creator { get; set; }

        public virtual ICollection<PromoUsage> PromoUsages { get; set; } = new List<PromoUsage>();
    }

    [Table("promo_usages")]
    public class PromoUsage
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("promo_id")]
        public Guid PromoId { get; set; }

        [Required]
        [Column("user_id")]
        public Guid UserId { get; set; }

        [Required]
        [Column("transaction_id")]
        public Guid TransactionId { get; set; }

        [Required]
        [Column("purchase_amount", TypeName = "decimal(10,2)")]
        public decimal PurchaseAmount { get; set; }

        [Required]
        [Column("bonus_amount", TypeName = "decimal(10,2)")]
        public decimal BonusAmount { get; set; }

        [Column("used_at")]
        public DateTime UsedAt { get; set; } = DateTime.UtcNow;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("PromoId")]
        public virtual Promo Promo { get; set; } = null!;

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("TransactionId")]
        public virtual CreditTransaction Transaction { get; set; } = null!;
    }
}

