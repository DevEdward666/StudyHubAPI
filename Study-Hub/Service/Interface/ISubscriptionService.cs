using Study_Hub.Models.DTOs;

namespace StudyHubApi.Services.Interfaces
{
    public interface ISubscriptionService
    {
        // Package Management
        Task<List<SubscriptionPackageDto>> GetAllPackagesAsync();
        Task<List<SubscriptionPackageDto>> GetActivePackagesAsync();
        Task<SubscriptionPackageDto?> GetPackageByIdAsync(Guid packageId);
        Task<SubscriptionPackageDto> CreatePackageAsync(CreateSubscriptionPackageDto dto, Guid createdBy);
        Task<SubscriptionPackageDto> UpdatePackageAsync(Guid packageId, UpdateSubscriptionPackageDto dto);
        Task<bool> DeletePackageAsync(Guid packageId);

        // User Subscription Management
        Task<List<UserSubscriptionDto>> GetUserSubscriptionsAsync(Guid userId);
        Task<List<UserSubscriptionDto>> GetActiveUserSubscriptionsAsync(Guid userId);
        Task<UserSubscriptionDto?> GetSubscriptionByIdAsync(Guid subscriptionId);
        Task<UserSubscriptionDto> PurchaseSubscriptionAsync(Guid userId, PurchaseSubscriptionDto dto);
        Task<UserSubscriptionDto> AdminPurchaseSubscriptionAsync(AdminPurchaseSubscriptionDto dto, Guid adminId);
        Task<bool> CancelSubscriptionAsync(Guid subscriptionId, Guid userId);
        Task<SubscriptionUsageDto> GetSubscriptionUsageAsync(Guid subscriptionId);

        // Admin functions
        Task<List<UserSubscriptionWithUserDto>> GetAllUserSubscriptionsAsync();
        Task<List<UserSubscriptionWithUserDto>> GetSubscriptionsByStatusAsync(string status);
    }
}

