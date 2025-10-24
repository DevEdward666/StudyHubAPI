using Microsoft.EntityFrameworkCore;
using Study_Hub.Data;
using Study_Hub.Models.DTOs;
using Study_Hub.Models.Entities;
using Study_Hub.Service.Interface;
using System.Text.Json;

namespace Study_Hub.Service
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;

        public NotificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        #region Create Notifications

        public async Task<NotificationDto> CreateNotificationAsync(CreateNotificationDto createDto)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = createDto.UserId,
                Title = createDto.Title,
                Message = createDto.Message,
                Type = createDto.Type,
                Data = createDto.Data,
                ActionUrl = createDto.ActionUrl,
                Priority = createDto.Priority,
                ExpiresAt = createDto.ExpiresAt,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return MapToNotificationDto(notification);
        }

        public async Task<List<NotificationDto>> CreateBulkNotificationsAsync(BulkNotificationDto bulkDto)
        {
            var notifications = new List<Notification>();
            var createdAt = DateTime.UtcNow;

            foreach (var userId in bulkDto.UserIds)
            {
                var notification = new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Title = bulkDto.Title,
                    Message = bulkDto.Message,
                    Type = bulkDto.Type,
                    Data = bulkDto.Data,
                    ActionUrl = bulkDto.ActionUrl,
                    Priority = bulkDto.Priority,
                    ExpiresAt = bulkDto.ExpiresAt,
                    CreatedAt = createdAt,
                    UpdatedAt = createdAt
                };

                notifications.Add(notification);
            }

            _context.Notifications.AddRange(notifications);
            await _context.SaveChangesAsync();

            return notifications.Select(MapToNotificationDto).ToList();
        }

        #endregion

        #region Get Notifications

        public async Task<PaginatedNotificationsDto> GetUserNotificationsAsync(Guid userId, NotificationFilterDto filter)
        {
            var query = _context.Notifications
                .Where(n => n.UserId == userId);

            // Apply filters
            if (filter.Type.HasValue)
                query = query.Where(n => n.Type == filter.Type.Value);

            if (filter.IsRead.HasValue)
                query = query.Where(n => n.IsRead == filter.IsRead.Value);

            if (filter.Priority.HasValue)
                query = query.Where(n => n.Priority == filter.Priority.Value);

            if (filter.FromDate.HasValue)
                query = query.Where(n => n.CreatedAt >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                query = query.Where(n => n.CreatedAt <= filter.ToDate.Value);

            // Filter out expired notifications
            query = query.Where(n => n.ExpiresAt == null || n.ExpiresAt > DateTime.UtcNow);

            var totalCount = await query.CountAsync();

            var notifications = await query
                .OrderByDescending(n => n.Priority)
                .ThenByDescending(n => n.CreatedAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var totalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize);

            return new PaginatedNotificationsDto
            {
                Notifications = notifications.Select(MapToNotificationDto).ToList(),
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize,
                TotalPages = totalPages,
                HasNextPage = filter.Page < totalPages,
                HasPreviousPage = filter.Page > 1
            };
        }

        public async Task<NotificationDto?> GetNotificationByIdAsync(Guid notificationId, Guid userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            return notification != null ? MapToNotificationDto(notification) : null;
        }

        public async Task<NotificationSummaryDto> GetNotificationSummaryAsync(Guid userId)
        {
            var query = _context.Notifications
                .Where(n => n.UserId == userId)
                .Where(n => n.ExpiresAt == null || n.ExpiresAt > DateTime.UtcNow);

            var totalCount = await query.CountAsync();
            var unreadCount = await query.CountAsync(n => !n.IsRead);
            var todayCount = await query.CountAsync(n => n.CreatedAt.Date == DateTime.UtcNow.Date);
            var highPriorityCount = await query.CountAsync(n => n.Priority == NotificationPriority.High);
            var criticalCount = await query.CountAsync(n => n.Priority == NotificationPriority.Critical);

            var countByType = await query
                .GroupBy(n => n.Type)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Type, x => x.Count);

            return new NotificationSummaryDto
            {
                TotalCount = totalCount,
                UnreadCount = unreadCount,
                TodayCount = todayCount,
                HighPriorityCount = highPriorityCount,
                CriticalCount = criticalCount,
                CountByType = countByType
            };
        }

        public async Task<List<NotificationDto>> GetUnreadNotificationsAsync(Guid userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .Where(n => n.ExpiresAt == null || n.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(n => n.Priority)
                .ThenByDescending(n => n.CreatedAt)
                .ToListAsync();

            return notifications.Select(MapToNotificationDto).ToList();
        }

        #endregion

        #region Update Notifications

        public async Task<NotificationDto?> UpdateNotificationAsync(Guid notificationId, UpdateNotificationDto updateDto, Guid userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification == null)
                return null;

            if (!string.IsNullOrWhiteSpace(updateDto.Title))
                notification.Title = updateDto.Title;

            if (!string.IsNullOrWhiteSpace(updateDto.Message))
                notification.Message = updateDto.Message;

            if (updateDto.Type.HasValue)
                notification.Type = updateDto.Type.Value;

            if (updateDto.Data != null)
                notification.Data = updateDto.Data;

            if (updateDto.ActionUrl != null)
                notification.ActionUrl = updateDto.ActionUrl;

            if (updateDto.Priority.HasValue)
                notification.Priority = updateDto.Priority.Value;

            if (updateDto.ExpiresAt.HasValue)
                notification.ExpiresAt = updateDto.ExpiresAt.Value;

            notification.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return MapToNotificationDto(notification);
        }

        public async Task<bool> MarkNotificationsAsReadAsync(Guid userId, MarkNotificationReadDto markReadDto)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && markReadDto.NotificationIds.Contains(n.Id) && !n.IsRead)
                .ToListAsync();

            if (!notifications.Any())
                return false;

            var readAt = DateTime.UtcNow;
            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                notification.ReadAt = readAt;
                notification.UpdatedAt = readAt;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkAllNotificationsAsReadAsync(Guid userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            if (!notifications.Any())
                return false;

            var readAt = DateTime.UtcNow;
            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                notification.ReadAt = readAt;
                notification.UpdatedAt = readAt;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkNotificationAsReadAsync(Guid notificationId, Guid userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId && !n.IsRead);

            if (notification == null)
                return false;

            var readAt = DateTime.UtcNow;
            notification.IsRead = true;
            notification.ReadAt = readAt;
            notification.UpdatedAt = readAt;

            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Delete Notifications

        public async Task<bool> DeleteNotificationAsync(Guid notificationId, Guid userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification == null)
                return false;

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteExpiredNotificationsAsync()
        {
            var expiredNotifications = await _context.Notifications
                .Where(n => n.ExpiresAt.HasValue && n.ExpiresAt < DateTime.UtcNow)
                .ToListAsync();

            if (!expiredNotifications.Any())
                return false;

            _context.Notifications.RemoveRange(expiredNotifications);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> DeleteOldNotificationsAsync(DateTime cutoffDate)
        {
            var oldNotifications = await _context.Notifications
                .Where(n => n.CreatedAt < cutoffDate)
                .ToListAsync();

            if (!oldNotifications.Any())
                return 0;

            _context.Notifications.RemoveRange(oldNotifications);
            await _context.SaveChangesAsync();
            return oldNotifications.Count;
        }

        #endregion

        #region Utility Methods

        public async Task<bool> NotificationExistsAsync(Guid notificationId, Guid userId)
        {
            return await _context.Notifications
                .AnyAsync(n => n.Id == notificationId && n.UserId == userId);
        }

        public async Task<int> GetUnreadCountAsync(Guid userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead &&
                               (n.ExpiresAt == null || n.ExpiresAt > DateTime.UtcNow));
        }

        #endregion

        #region System Notifications

        public async Task SendCreditPurchaseNotificationAsync(Guid userId, decimal amount, string transactionId)
        {
            var createDto = new CreateNotificationDto
            {
                UserId = userId,
                Title = "Credits Purchased Successfully",
                Message = $"You have successfully purchased {amount} credits. Transaction ID: {transactionId}",
                Type = NotificationType.Credit,
                Priority = NotificationPriority.Normal,
                Data = JsonSerializer.Serialize(new { Amount = amount, TransactionId = transactionId })
            };

            await CreateNotificationAsync(createDto);
        }

        public async Task SendSessionStartNotificationAsync(Guid userId, string tableName, DateTime startTime)
        {
            var createDto = new CreateNotificationDto
            {
                UserId = userId,
                Title = "Study Session Started",
                Message = $"Your study session at {tableName} has started at {startTime:HH:mm}",
                Type = NotificationType.Session,
                Priority = NotificationPriority.Normal,
                Data = JsonSerializer.Serialize(new { TableName = tableName, StartTime = startTime })
            };

            await CreateNotificationAsync(createDto);
        }

        public async Task SendSessionEndNotificationAsync(Guid userId, string tableName, DateTime endTime, decimal creditsUsed)
        {
            var createDto = new CreateNotificationDto
            {
                UserId = userId,
                Title = "Study Session Ended",
                Message = $"Your study session at {tableName} has ended. Credits used: {creditsUsed}",
                Type = NotificationType.Session,
                Priority = NotificationPriority.Normal,
                Data = JsonSerializer.Serialize(new { TableName = tableName, EndTime = endTime, CreditsUsed = creditsUsed })
            };

            await CreateNotificationAsync(createDto);
        }

        public async Task SendLowCreditNotificationAsync(Guid userId, decimal currentBalance)
        {
            var createDto = new CreateNotificationDto
            {
                UserId = userId,
                Title = "Low Credit Balance",
                Message = $"Your credit balance is running low ({currentBalance} credits remaining). Consider purchasing more credits.",
                Type = NotificationType.Credit,
                Priority = NotificationPriority.High,
                ActionUrl = "/credits/purchase",
                Data = JsonSerializer.Serialize(new { CurrentBalance = currentBalance })
            };

            await CreateNotificationAsync(createDto);
        }

        public async Task SendWelcomeNotificationAsync(Guid userId, string userName)
        {
            var createDto = new CreateNotificationDto
            {
                UserId = userId,
                Title = "Welcome to Study Hub!",
                Message = $"Welcome {userName}! We're excited to have you join our study community. Get started by exploring available study tables.",
                Type = NotificationType.System,
                Priority = NotificationPriority.Normal,
                ActionUrl = "/tables"
            };

            await CreateNotificationAsync(createDto);
        }

        public async Task SendSystemMaintenanceNotificationAsync(List<Guid> userIds, DateTime maintenanceTime)
        {
            var bulkDto = new BulkNotificationDto
            {
                UserIds = userIds,
                Title = "Scheduled System Maintenance",
                Message = $"System maintenance is scheduled for {maintenanceTime:yyyy-MM-dd HH:mm}. Services may be temporarily unavailable.",
                Type = NotificationType.Alert,
                Priority = NotificationPriority.High,
                ExpiresAt = maintenanceTime.AddHours(2)
            };

            await CreateBulkNotificationsAsync(bulkDto);
        }

        public async Task SendPromotionNotificationAsync(List<Guid> userIds, string promotionTitle, string promotionMessage, string? actionUrl = null)
        {
            var bulkDto = new BulkNotificationDto
            {
                UserIds = userIds,
                Title = promotionTitle,
                Message = promotionMessage,
                Type = NotificationType.Promotion,
                Priority = NotificationPriority.Normal,
                ActionUrl = actionUrl,
                ExpiresAt = DateTime.UtcNow.AddDays(7) // Promotions expire in 7 days
            };

            await CreateBulkNotificationsAsync(bulkDto);
        }

        #endregion

        #region Helper Methods

        private static NotificationDto MapToNotificationDto(Notification notification)
        {
            return new NotificationDto
            {
                Id = notification.Id,
                UserId = notification.UserId,
                Title = notification.Title,
                Message = notification.Message,
                Type = notification.Type,
                IsRead = notification.IsRead,
                Data = notification.Data,
                ActionUrl = notification.ActionUrl,
                Priority = notification.Priority,
                ExpiresAt = notification.ExpiresAt,
                CreatedAt = notification.CreatedAt,
                UpdatedAt = notification.UpdatedAt,
                ReadAt = notification.ReadAt
            };
        }

        #endregion
    }
}