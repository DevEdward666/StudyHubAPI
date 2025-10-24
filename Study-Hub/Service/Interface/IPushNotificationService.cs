using Study_Hub.Models.DTOs;

namespace Study_Hub.Service.Interface
{
    public interface IPushNotificationService
    {
        // Subscription Management
        Task<PushSubscriptionResponseDto> SubscribeAsync(Guid userId, PushSubscriptionDto subscription);
        Task<bool> UnsubscribeAsync(Guid userId, string endpoint);
        Task<List<PushSubscriptionResponseDto>> GetUserSubscriptionsAsync(Guid userId);
        Task<bool> UpdateSubscriptionStatusAsync(Guid subscriptionId, bool isActive);

        // Push Notifications
        Task<bool> SendPushNotificationAsync(Guid userId, PushNotificationDto notification);
        Task<int> SendPushNotificationToAllUsersAsync(PushNotificationDto notification);
        Task<int> SendPushNotificationToUsersAsync(List<Guid> userIds, PushNotificationDto notification);
        
        // Test
        Task<bool> SendTestPushAsync(Guid userId, string title, string body);

        // VAPID Keys
        string GetVapidPublicKey();
        
        // Cleanup
        Task<int> CleanupInactiveSubscriptionsAsync(int inactiveDays = 90);
    }
}

