namespace Study_Hub.Models.DTOs
{
    // Subscription Package DTOs
    public class SubscriptionPackageDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string PackageType { get; set; } // Hourly, Daily, Weekly, Monthly
        public int DurationValue { get; set; }
        public decimal TotalHours { get; set; }
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateSubscriptionPackageDto
    {
        public string Name { get; set; }
        public string PackageType { get; set; } // Hourly, Daily, Weekly, Monthly
        public int DurationValue { get; set; }
        public decimal TotalHours { get; set; }
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class UpdateSubscriptionPackageDto
    {
        public string? Name { get; set; }
        public decimal? Price { get; set; }
        public string? Description { get; set; }
        public bool? IsActive { get; set; }
        public int? DisplayOrder { get; set; }
    }

    // User Subscription DTOs
    public class UserSubscriptionDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid PackageId { get; set; }
        public string PackageName { get; set; }
        public string PackageType { get; set; }
        public decimal TotalHours { get; set; }
        public decimal RemainingHours { get; set; }
        public decimal HoursUsed { get; set; }
        public decimal PercentageUsed { get; set; }
        public DateTime PurchaseDate { get; set; }
        public DateTime? ActivationDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string Status { get; set; }
        public decimal PurchaseAmount { get; set; }
        public string? PaymentMethod { get; set; }
        public string? TransactionReference { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? CreatedByName { get; set; }
    }

    public class UserSubscriptionWithUserDto : UserSubscriptionDto
    {
        public UserDto User { get; set; }
    }

    public class PurchaseSubscriptionDto
    {
        public Guid PackageId { get; set; }
        public string PaymentMethod { get; set; }
        public decimal Cash { get; set; }
        public decimal Change { get; set; }
        public string? TransactionReference { get; set; }
        public string? Notes { get; set; }
    }

    public class AdminPurchaseSubscriptionDto
    {
        public Guid UserId { get; set; }
        public Guid PackageId { get; set; }
        public string PaymentMethod { get; set; }
        public decimal? Cash { get; set; }
        public decimal? Change { get; set; }
        public string? TransactionReference { get; set; }
        public string? Notes { get; set; }
    }

    public class StartSubscriptionSessionDto
    {
        public Guid TableId { get; set; }
        public Guid SubscriptionId { get; set; }
        public string? UserId { get; set; } // For admin to assign user
    }

    public class SubscriptionUsageDto
    {
        public Guid SubscriptionId { get; set; }
        public decimal HoursConsumed { get; set; }
        public decimal RemainingHours { get; set; }
        public DateTime SessionStartTime { get; set; }
        public DateTime? SessionEndTime { get; set; }
    }
}

