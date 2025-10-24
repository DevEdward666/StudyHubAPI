using Study_Hub.Models.DTOs;

namespace Study_Hub.Service.Interface
{
    public interface INotificationService
    {
        // Create notifications
        Task<NotificationDto> CreateNotificationAsync(CreateNotificationDto createDto);
        Task<List<NotificationDto>> CreateBulkNotificationsAsync(BulkNotificationDto bulkDto);

        // Get notifications
        Task<PaginatedNotificationsDto> GetUserNotificationsAsync(Guid userId, NotificationFilterDto filter);
        Task<NotificationDto?> GetNotificationByIdAsync(Guid notificationId, Guid userId);
        Task<NotificationSummaryDto> GetNotificationSummaryAsync(Guid userId);
        Task<List<NotificationDto>> GetUnreadNotificationsAsync(Guid userId);

        // Update notifications
        Task<NotificationDto?> UpdateNotificationAsync(Guid notificationId, UpdateNotificationDto updateDto, Guid userId);
        Task<bool> MarkNotificationsAsReadAsync(Guid userId, MarkNotificationReadDto markReadDto);
        Task<bool> MarkAllNotificationsAsReadAsync(Guid userId);
        Task<bool> MarkNotificationAsReadAsync(Guid notificationId, Guid userId);

        // Delete notifications
        Task<bool> DeleteNotificationAsync(Guid notificationId, Guid userId);
        Task<bool> DeleteExpiredNotificationsAsync();
        Task<int> DeleteOldNotificationsAsync(DateTime cutoffDate);

        // Utility methods
        Task<bool> NotificationExistsAsync(Guid notificationId, Guid userId);
        Task<int> GetUnreadCountAsync(Guid userId);

        // System notifications (for internal use)
        Task SendCreditPurchaseNotificationAsync(Guid userId, decimal amount, string transactionId);
        Task SendSessionStartNotificationAsync(Guid userId, string tableName, DateTime startTime);
        Task SendSessionEndNotificationAsync(Guid userId, string tableName, DateTime endTime, decimal creditsUsed);
        Task SendLowCreditNotificationAsync(Guid userId, decimal currentBalance);
        Task SendWelcomeNotificationAsync(Guid userId, string userName);
        Task SendSystemMaintenanceNotificationAsync(List<Guid> userIds, DateTime maintenanceTime);
        Task SendPromotionNotificationAsync(List<Guid> userIds, string promotionTitle, string promotionMessage, string? actionUrl = null);
    }
}