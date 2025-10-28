using Study_Hub.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace Study_Hub.Models.DTOs
{
    // Create Promo
    public class CreatePromoRequestDto
    {
        [Required]
        [StringLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public PromoType Type { get; set; }

        // Bonus values based on type
        [Range(0, 100)]
        public decimal? PercentageBonus { get; set; } // For Percentage type (0-100)

        [Range(0.01, double.MaxValue)]
        public decimal? FixedBonusAmount { get; set; } // For FixedAmount type

        [Range(0.01, double.MaxValue)]
        public decimal? BuyAmount { get; set; } // For BuyXGetY type

        [Range(0.01, double.MaxValue)]
        public decimal? GetAmount { get; set; } // For BuyXGetY type

        // Constraints
        [Range(0.01, double.MaxValue)]
        public decimal? MinPurchaseAmount { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal? MaxDiscountAmount { get; set; }

        [Range(1, int.MaxValue)]
        public int? UsageLimit { get; set; }

        [Range(1, int.MaxValue)]
        public int? UsagePerUser { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    // Update Promo
    public class UpdatePromoRequestDto
    {
        [Required]
        public Guid PromoId { get; set; }

        [StringLength(200)]
        public string? Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public PromoStatus? Status { get; set; }

        // Bonus values
        [Range(0, 100)]
        public decimal? PercentageBonus { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal? FixedBonusAmount { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal? BuyAmount { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal? GetAmount { get; set; }

        // Constraints
        [Range(0.01, double.MaxValue)]
        public decimal? MinPurchaseAmount { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal? MaxDiscountAmount { get; set; }

        [Range(1, int.MaxValue)]
        public int? UsageLimit { get; set; }

        [Range(1, int.MaxValue)]
        public int? UsagePerUser { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    // Promo Response
    public class PromoDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public PromoType Type { get; set; }
        public PromoStatus Status { get; set; }

        public decimal? PercentageBonus { get; set; }
        public decimal? FixedBonusAmount { get; set; }
        public decimal? BuyAmount { get; set; }
        public decimal? GetAmount { get; set; }

        public decimal? MinPurchaseAmount { get; set; }
        public decimal? MaxDiscountAmount { get; set; }
        public int? UsageLimit { get; set; }
        public int? UsagePerUser { get; set; }
        public int CurrentUsageCount { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public string? CreatorEmail { get; set; }
    }

    // Apply Promo (when user makes a purchase)
    public class ApplyPromoRequestDto
    {
        [Required]
        public string PromoCode { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal PurchaseAmount { get; set; }
    }

    public class ApplyPromoResponseDto
    {
        public bool IsValid { get; set; }
        public string? Message { get; set; }
        public decimal OriginalAmount { get; set; }
        public decimal BonusAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public PromoDto? PromoDetails { get; set; }
    }

    // Promo Usage History
    public class PromoUsageDto
    {
        public Guid Id { get; set; }
        public Guid PromoId { get; set; }
        public string PromoCode { get; set; } = string.Empty;
        public string PromoName { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public Guid TransactionId { get; set; }
        public decimal PurchaseAmount { get; set; }
        public decimal BonusAmount { get; set; }
        public DateTime UsedAt { get; set; }
    }

    // Delete/Toggle Promo
    public class DeletePromoRequestDto
    {
        [Required]
        public Guid PromoId { get; set; }
    }

    public class TogglePromoStatusRequestDto
    {
        [Required]
        public Guid PromoId { get; set; }

        [Required]
        public PromoStatus Status { get; set; }
    }

    // Validate Promo (check if code is valid before applying)
    public class ValidatePromoRequestDto
    {
        [Required]
        public string PromoCode { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal PurchaseAmount { get; set; }
    }

    // Promo Statistics
    public class PromoStatisticsDto
    {
        public Guid PromoId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int TotalUsageCount { get; set; }
        public int UniqueUsersCount { get; set; }
        public decimal TotalBonusGiven { get; set; }
        public decimal TotalPurchaseAmount { get; set; }
        public DateTime? LastUsedAt { get; set; }
    }
}

